using System;
using System.Collections.Generic;
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
        private const string PRE_PATH = "https://opensheet.elk.sh/";
        private static readonly string ROOT_FORMAT_PATH = "https://docs.google.com/spreadsheets/d/{0}/edit";

        [MenuItem("Tools/Config Downloader")]
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

                string key = EditorPrefsKeyPrefix + "/download_asset_folder";
                string folder = EditorPrefs.GetString(key);
                string newFolder = EditorGUILayout.TextField(folder);
                if (newFolder != folder)
                {
                    EditorPrefs.SetString(key, newFolder);
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
                FieldInfo[] assetFields = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool found = false;
                foreach (FieldInfo info in assetFields)
                {
                    bool flag = !info.FieldType.IsGenericType ||
                                info.FieldType.GetGenericTypeDefinition() != typeof(List<>);
                    if (!flag)
                    {
                        found = true;
                        UnityWebRequestAsyncOperation operation = UnityWebRequest
                            .Get(PRE_PATH + urlMap[fullName] + "/" + info.Name).SendWebRequest();
                        while (!operation.isDone)
                        {
                            await Task.Delay(1000);
                        }

                        UnityWebRequest request = operation.webRequest;
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            Debug.Log(request.downloadHandler.text);
                            /*Type listType = info.FieldType;
                            Type elementType = listType.GetGenericArguments()[0];
                            object defaultValue = Activator.CreateInstance(elementType);
                            Type wrapperType = typeof(ListWrapper<>).MakeGenericType(elementType);
                            string wrappedJson = "{ \"data\": " + request.downloadHandler.text + " }";
                            object wrapper = JsonUtility.FromJson(wrappedJson, wrapperType);
                            object listValue = wrapperType.GetField("data",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                            ).GetValue(wrapper);
                            IList list = listValue as IList;
                            int count = list.Count;
                            for (int i = count - 1; i >= 0; i--)
                            {
                                object item = list[i];
                                if (Equals(item, defaultValue)) list.RemoveAt(i);
                            }

                            info.SetValue(config, list);
                            EditorUtility.SetDirty(target);
                            AssetDatabase.SaveAssets();*/
                        }
                        else
                        {
                            Debug.LogError("Error download " + info.Name + ", detail:: " + request.error + "\n" +
                                           request.result);
                        }
                    }
                }

                if (!found)
                {
                    Debug.LogError(
                        "Không có kiểu phù hợp để tìm Spread-Sheet, Yêu cầu object phải sử dụng Atribute [SerializeField] và là có kiểu là List<T>");
                }
            }
            catch (Exception ex)
            {
                statusMap[fullName] = $"ERROR: {ex.Message}";
            }
            finally
            {
                downloading.Remove(fullName);
                Repaint();
            }
        }

        private void SaveConfigToAsset(Type type, string json)
        {
            var folderPath = "Assets/_FightCode/Config/Downloads";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/_FightCode/Config", "Downloads");
            }

            var assetPath = $"{folderPath}/{type.Name}.json";
            System.IO.File.WriteAllText(assetPath, json);
            AssetDatabase.ImportAsset(assetPath);

            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (textAsset != null)
            {
                EditorGUIUtility.PingObject(textAsset);
            }

            AssetDatabase.Refresh();
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
    }
}
