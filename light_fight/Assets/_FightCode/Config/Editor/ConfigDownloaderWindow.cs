using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using _KITSystem.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace _FightCode.Config.Editor
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
        private const string EditorPrefsKeyFolderPrefix = "ConfigDownloader_Folder";
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

                GUILayout.Space(50);

                string folder = EditorPrefs.GetString(EditorPrefsKeyFolderPrefix);
                string newFolder = EditorGUILayout.TextField(folder);
                if (newFolder != folder)
                {
                    EditorPrefs.SetString(EditorPrefsKeyFolderPrefix, newFolder);
                }
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
                            DownloadConfig(type);
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

        private async void DownloadConfig(Type type)
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
                object target = Activator.CreateInstance(type);
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
                        Debug.LogError($"Failed to download {info.Name}: {request.error}");
                        request.Dispose();
                        continue;
                    }

                    var json = request.downloadHandler.text;
                    request.Dispose();

                    var elementType = info.FieldType.GetGenericArguments()[0];
                    var wrapperType = typeof(ListWrapper<>).MakeGenericType(elementType);
                    var wrappedJson = "{ \"data\": " + json + " }";
                    var wrapper = JsonUtility.FromJson(wrappedJson, wrapperType);
                    var listValue = wrapperType.GetField("data").GetValue(wrapper);
                    var list = listValue as IList;
                    var defaultValue = Activator.CreateInstance(elementType);
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (Equals(list[i], defaultValue))
                            list.RemoveAt(i);
                    }
                    info.SetValue(target, list);
                }

                var projectPath = Path.GetDirectoryName(Application.dataPath);
                var folder = EditorPrefs.GetString(EditorPrefsKeyFolderPrefix);
                var fileName = type.Name + ".json";
                var savePath = Path.Combine(projectPath, folder, fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                await File.WriteAllTextAsync(savePath, JsonUtility.ToJson(target, true));

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
            }
        }

        private void DownloadAll()
        {
            foreach (var type in configTypes)
            {
                DownloadConfig(type);
            }
        }

        private static string GetPrefsKey(Type type)
        {
            return $"{EditorPrefsKeyPrefix}{type.FullName}";
        }
        
        [Serializable]
        class ListWrapper<T>
        {
            public List<T> data = new List<T>();
        }
    }
}
