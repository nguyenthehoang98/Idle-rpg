#if UNITY_EDITOR
 using System;
 using System.Collections.Generic;
 using System.Reflection;
 using System.Threading.Tasks;
 using UnityEditor;
 using UnityEngine;
 using UnityEngine.Networking;

 namespace _KITSystem.ExcelConfig
{
    [Serializable]
    class ListWrapper<T>
    {
        public List<T> data;
    }
    
    [CustomEditor(typeof(KitBaseConfig), true)]
    class KitConfigInspector : UnityEditor.Editor
    {
        private const string PRE_PATH = "https://opensheet.elk.sh/";
        private static readonly string ROOT_FORMAT_PATH = "https://docs.google.com/spreadsheets/d/{0}/edit";
        private KitBaseConfig config;

        private void OnEnable()
        {
            config = target as KitBaseConfig;
        }

        public override void OnInspectorGUI()
        {
            int widthButtonMini = 80;
            int widthLabel = 80;
            
            EditorGUILayout.LabelField("Google Sheet Link Config");
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
                    
            EditorGUILayout.BeginVertical();
            // todo:
            if (GUILayout.Button("Import", GUILayout.Width(widthButtonMini))) Download();
            if (GUILayout.Button("Open", GUILayout.Width(widthButtonMini)))
            {
                string sheetId = config.fileUrl.Split("/d/")[1].Split('/')[0];
                Application.OpenURL(string.Format(ROOT_FORMAT_PATH, sheetId));
            }
            EditorGUILayout.EndVertical();
                    
            EditorGUILayout.BeginVertical();
            // todo:
            EditorGUILayout.LabelField("URL", "", GUILayout.Width(widthLabel));
            GUI.enabled = config.enable;
            config.fileUrl = EditorGUILayout.TextField("", config.fileUrl);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
                    
            EditorGUILayout.BeginVertical();
            // todo:
            EditorGUILayout.LabelField("Enable", "", GUILayout.Width(widthLabel));
            config.enable = EditorGUILayout.Toggle("", config.enable);
            EditorGUILayout.EndHorizontal();
                    
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            config.OnGuiEnable();
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }

        private async void Download()
        {
            Debug.Log($"Downloading {config.GetType().Name}...");
            FieldInfo[] assetFields = config.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool found = false;
            foreach (FieldInfo info in assetFields)
            {
                bool flag = !info.FieldType.IsGenericType || info.FieldType.GetGenericTypeDefinition() != typeof(List<>);
                if (!flag)
                {
                    found = true;
                    string sheetId = config.fileUrl.Split("/d/")[1].Split('/')[0];
                    UnityWebRequestAsyncOperation operation = UnityWebRequest.Get(PRE_PATH + sheetId + "/" + info.Name).SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Delay(1000);
                    }

                    UnityWebRequest request = operation.webRequest;
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Type listType = info.FieldType;
                        Type elementType = listType.GetGenericArguments()[0];
                        Type wrapperType = typeof(ListWrapper<>).MakeGenericType(elementType);
                        string wrappedJson = "{ \"data\": " + request.downloadHandler.text + " }";
                        object wrapper = JsonUtility.FromJson(wrappedJson, wrapperType);
                        object listValue = wrapperType.GetField(
                            "data",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                        ).GetValue(wrapper);
                        info.SetValue(config, listValue);
                        EditorUtility.SetDirty(target);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        Debug.LogError("Error download " + info.Name + ", detail:: " + request.error + "\n" + request.result);
                    }
                }
            }
            if (!found)
            {
                Debug.LogError("Không có kiểu phù hợp để tìm Spread-Sheet, Yêu cầu object phải sử dụng Atribute [SerializeField] và là có kiểu là List<T>");
            }
            Debug.Log($"Completed {config.GetType().Name}");
        }
    }
}
#endif