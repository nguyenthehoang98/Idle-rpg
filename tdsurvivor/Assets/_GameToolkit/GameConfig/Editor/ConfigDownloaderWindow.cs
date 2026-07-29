using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Codice.Client.BaseCommands;
using K4os.Compression.LZ4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace _KITSystem.Config.Editor
{
    public class ConfigDownloaderWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private List<Type> configTypes;
        private Dictionary<string, string> urlMap = new Dictionary<string, string>();
        private Dictionary<string, string> statusMap = new Dictionary<string, string>();
        private Dictionary<string, bool> enable = new Dictionary<string, bool>();
        private HashSet<string> downloading = new HashSet<string>();

        private const string EditorPrefsKeyPrefix = "ConfigDownloader_URL_";
        private const string PRE_PATH = "https://opensheet.elk.sh/";
        private static readonly string ROOT_FORMAT_PATH = "https://docs.google.com/spreadsheets/d/{0}/edit";

        [MenuItem("Tools/Config/Downloader")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigDownloaderWindow>("Config Downloader");
            window.Show();
        }

        private void OnEnable()
        {
            RefreshTypeList();
        }

        private void RefreshTypeList()
        {
            configTypes = TypeCache.GetTypesDerivedFrom<IGameConfig>()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name)
                .ToList();

            urlMap.Clear();
            statusMap.Clear();

            foreach (var type in configTypes)
            {
                var key = GetPrefsKey(type);
                urlMap[type.FullName] = EditorPrefs.GetString(key, "");
                statusMap[type.FullName] = "";
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(8);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Download All", GUILayout.Width(100)))
                {
                    DownloadAll();
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Validate All", GUILayout.Width(100)))
                {
                    ValidateAll();
                }

                GUILayout.Space(10);

                var folder = ConfigPath.Folder;

                var newFolder = EditorGUILayout.TextField(folder);

                if (newFolder != folder) ConfigPath.Folder = newFolder;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(4);
                GUILayout.Label($"Found {configTypes.Count} config types", EditorStyles.miniLabel);

                GUILayout.Space(4);
                GUILayout.Label("Asset folder", EditorStyles.miniLabel);
            }
            
            GUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var type in configTypes)
            {
                DrawConfigRow(type);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawConfigRow(Type type)
        {
            var fullName = type.FullName;
            var key = GetPrefsKey(type);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                bool e;
                if (!enable.TryGetValue(fullName, out e))
                {
                    enable.Add(fullName, e);
                }
                
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(type.Name, EditorStyles.boldLabel, GUILayout.Width(180));
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(e ? "Close" : "Open", GUILayout.Width(80)))
                    {
                        enable[fullName] = !enable[fullName];
                        e = enable[fullName];
                    }

                    var isDownloading = downloading.Contains(fullName);

                    using (new EditorGUI.DisabledGroupScope(isDownloading))
                    {
                        if (GUILayout.Button(isDownloading ? "..." : "Download", GUILayout.Width(80)))
                        {
                            DownloadConfig(type, ValidateAll);
                        }
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(50)))
                    {
                        urlMap[fullName] = "";
                        EditorPrefs.DeleteKey(key);
                        statusMap[fullName] = "";
                    }
                }

                GUI.SetNextControlName(fullName);

                GUI.enabled = e;
                
                var newUrl = EditorGUILayout.TextField("URL", urlMap[fullName]);

                GUI.enabled = true;
                
                if (newUrl != urlMap[fullName])
                {
                    urlMap[fullName] = newUrl;
                    EditorPrefs.SetString(key, newUrl);
                }

                var status = statusMap[fullName];
                if (!string.IsNullOrEmpty(status))
                {
                    var rect = EditorGUILayout.GetControlRect();
                    EditorGUI.HelpBox(rect, status, status.StartsWith("OK")
                        ? MessageType.Info
                        : MessageType.Error);
                }
            }

            GUILayout.Space(2);
        }

        private async void DownloadConfig(Type type, Action onComplete)
        {
            var fullName = type.FullName;
            if (downloading.Contains(fullName))
                return;
            var url = urlMap[fullName];

            if (string.IsNullOrWhiteSpace(url))
            {
                statusMap[fullName] = "ERROR: URL is empty";
                return;
            }

            downloading.Add(fullName);
            statusMap[fullName] = "Downloading...";
            Repaint();

            try
            {
                var target = Activator.CreateInstance(type);
                var listFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.FieldType.IsGenericType &&
                                f.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    .ToList();

                if (listFields.Count == 0)
                {
                    statusMap[fullName] = "ERROR: No List<T> fields found";
                    Debug.LogError("Config type must have at least one [SerializeField] List<T> field");
                    return;
                }

                var fieldsDone = 0;
                foreach (var info in listFields)
                {
                    statusMap[fullName] = $"Downloading {info.Name} ({++fieldsDone}/{listFields.Count})...";
                    Repaint();

                    var request = UnityWebRequest.Get(PRE_PATH + url + "/" + info.Name);
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                        await Task.Delay(100);

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to download '{info.Name}' error '{request.error}'");
                        request.Dispose();
                        continue;
                    }

                    var list = (IList)Activator.CreateInstance(info.FieldType);
                    
                    var elementType = info.FieldType.GetGenericArguments()[0];
                 
                    var json = request.downloadHandler.text;
                    
                    Debug.Log(json);

                    foreach (JObject token in JArray.Parse(json))
                    {
                        var item = Activator.CreateInstance(elementType);
                        
                        foreach (var field in elementType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
                            
                            if (!token.TryGetValue(field.Name, out var value)) continue;
                            
                            if (value.Type == JTokenType.Null) continue;

                            try
                            {
                                object fieldValue = ConvertValue(value, field.FieldType, field.Name);

                                field.SetValue(item, fieldValue);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"[{elementType.Name}] Field '{field.Name}' parse failed. Value={value}. Error={e.Message}");
                            }
                        }
                        
                        if (IsDefaultObject(item)) continue;

                        list.Add(item);
                    }

                    info.SetValue(target, list);

                    request.Dispose();
                }
                
                type.GetMethod("OnPostImported",
                        BindingFlags.Instance | BindingFlags.Public)
                    ?.Invoke(target, null);

                var projectPath = Path.GetDirectoryName(Application.dataPath);
                var folder = ConfigPath.Folder;
                var fileName = type.Name + ".json";
                var savePath = Path.Combine(projectPath, folder, fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                
                string contents = JsonUtility.ToJson(target, false);
                contents = Regex.Replace(
                    contents,
                    @"-?\d+\.\d+",
                    m =>
                    {
                        if (double.TryParse(
                                m.Value,
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out double d))
                        {
                            return d.ToString("0.####", CultureInfo.InvariantCulture);
                        }

                        return m.Value;
                    });
                
                byte[] bytes = Encoding.UTF8.GetBytes(contents);

                byte[] pack = LZ4Pickler.Pickle(bytes);

                await File.WriteAllBytesAsync(savePath, pack);

                var relativePath = savePath.Replace(projectPath, "").TrimStart(Path.DirectorySeparatorChar)
                    .Replace("\\", "/");
                AssetDatabase.ImportAsset(relativePath);
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }

                statusMap[fullName] = $"OK: Saved to {fileName}";
                Debug.Log($"Config saved: {savePath}");
            }
            catch (Exception ex)
            {
                statusMap[fullName] = $"ERROR: {ex.Message}";
                Debug.LogError(ex);
            }
            finally
            {
                downloading.Remove(fullName);
                Repaint();
                onComplete?.Invoke();
            }
        }
        
        private static bool IsDefaultObject(object obj)
        {
            if (obj == null)
                return true;

            var type = obj.GetType();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;

                var value = field.GetValue(obj);

                var defaultValue = field.FieldType.IsValueType
                    ? Activator.CreateInstance(field.FieldType)
                    : null;

                if (!Equals(value, defaultValue))
                    return false;
            }

            return true;
        }
        
        private static object ConvertValue(JToken token, Type targetType, string fieldName)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return targetType.IsValueType
                    ? Activator.CreateInstance(targetType)
                    : null;
            }

            string value = token.ToString();

            if (targetType == typeof(string))
            {
                return value;
            }

            if (targetType == typeof(int))
            {
                return int.TryParse(value, out int i) ? i : 0;
            }

            if (targetType == typeof(long))
            {
                return long.TryParse(value, out long l) ? l : 0;
            }

            if (targetType == typeof(float))
            {
                value = value.Replace(',', '.');
                
                float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f);

                return f;
            }

            if (targetType == typeof(double))
            {
                value = value.Replace(',', '.');
                
                double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double d);

                return d;
            }

            if (targetType == typeof(bool))
            {
                value = value.ToLower();

                return value switch
                {
                    "1" => true,
                    "0" => false,
                    "true" => true,
                    "false" => false,
                    "yes" => true,
                    "no" => false,
                    _ => false
                };
            }
            
            if (targetType.IsEnum)
            {
                return Enum.TryParse(targetType, value, true, out object enumValue)
                    ? enumValue
                    : Activator.CreateInstance(targetType);
            }

            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType();
                
                if (string.IsNullOrWhiteSpace(value))
                {
                    return Array.CreateInstance(elementType, 0);
                }

                JArray array = JArray.Parse(value);

                Array result = Array.CreateInstance(elementType, array.Count);

                for (int i = 0; i < array.Count; i++)
                {
                    result.SetValue(Convert.ChangeType(array[i].ToString(), elementType), i);
                }

                return result;
            }
            
            return token.ToObject(targetType);
        }

        private async void ValidateAll()
        {
            var folder = ConfigPath.Folder;
            
            var projectPath = Path.GetDirectoryName(Application.dataPath);

            foreach (var type in configTypes)
            {
                string fullName = type.FullName;
                
                statusMap[fullName] = $"Pending: Validate to {type.Name}.json";
                
                string filePath = Path.Combine(folder, type.Name + ".json");
                
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
                
                if (asset != null)
                {
                    byte[] unpick = LZ4Pickler.Unpickle(asset.bytes); 

                    string text = Encoding.UTF8.GetString(unpick);
                    
                    object target = JsonUtility.FromJson(text, type);
                    
                    if (target != null)
                    {
                        type.GetMethod("OnValidateLinkConfig",
                                BindingFlags.Instance | BindingFlags.Public)
                            ?.Invoke(target, null);
                        
                        string contents = JsonUtility.ToJson(target, false);
                        
                        contents = Regex.Replace(
                            contents,
                            @"-?\d+\.\d+",
                            m =>
                            {
                                if (double.TryParse(
                                        m.Value,
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out double d))
                                {
                                    return d.ToString("0.####", CultureInfo.InvariantCulture);
                                }

                                return m.Value;
                            });

                        string savePath = Path.Combine(projectPath, filePath);
                        
                        byte[] bytes = Encoding.UTF8.GetBytes(contents);

                        byte[] pack = LZ4Pickler.Pickle(bytes);
                        
                        await File.WriteAllBytesAsync(savePath, pack);
                        
                        AssetDatabase.ImportAsset(filePath);
                        
                        if (asset != null)
                        {
                            EditorGUIUtility.PingObject(asset);
                        }
                        
                        statusMap[fullName] = $"OK: Validate to {type.Name}.json";
                    }
                    else
                    {
                        statusMap[fullName] = $"ERROR: Error parse json to object {type}";
                    }
                }
                else
                {
                    statusMap[fullName] = $"ERROR: Not found config at '{filePath}'";
                }
            }
        }
        
        private void DownloadAll()
        {
            int count = 0;
            foreach (var type in configTypes)
            {
                DownloadConfig(type, () =>
                {
                    count++;

                    if (count == configTypes.Count) ValidateAll();
                });
            }
        }

        private static string GetPrefsKey(Type type)
        {
            return $"{EditorPrefsKeyPrefix}{type.FullName}";
        }
    }
}
