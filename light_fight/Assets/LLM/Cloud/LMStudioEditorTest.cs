using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Unity.EditorCoroutines.Editor;

public class LMStudioEditorTest : EditorWindow
{
    private string prompt = "Say hello from Unity Editor";
    private string response = "";

    private const string URL = "http://127.0.0.1:1234/v1/chat/completions";

    [MenuItem("Tools/LM Studio Test")]
    public static void ShowWindow()
    {
        GetWindow<LMStudioEditorTest>("LM Studio Test");
    }

    private string folderPath = "Assets/";

    void OnGUI()
    {
        GUILayout.Label("Folder Path:");
        folderPath = EditorGUILayout.TextField(folderPath);

        if (GUILayout.Button("Select Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                folderPath = path;
            }
        }

        if (GUILayout.Button("Analyze Code"))
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(AnalyzeFolder());
        }

        GUILayout.Label("Response:");
        EditorGUILayout.TextArea(response, GUILayout.Height(300));
    }
    
    string ReadAllCode(string folder)
    {
        var files = System.IO.Directory.GetFiles(folder, "*.cs", System.IO.SearchOption.AllDirectories);

        StringBuilder sb = new StringBuilder();

        foreach (var file in files)
        {
            string code = System.IO.File.ReadAllText(file);

            sb.AppendLine("===== FILE: " + file + " =====");
            sb.AppendLine(code);
            sb.AppendLine();
        }

        return sb.ToString();
    }
    
    IEnumerator AnalyzeFolder()
    {
        response = "Loading...";
        string code = ReadAllCode(folderPath);

        // ⚠️ tránh quá dài
        if (code.Length > 20000)
        {
            code = code.Substring(0, 20000);
        }

        string prompt = 
            "You are a senior Unity developer. Review the following C# code.\n" +
            "Give:\n" +
            "- Bugs\n" +
            "- Performance issues\n" +
            "- Clean code improvements\n\n" +
            code;

        yield return SendToLLM(prompt);
    }
    
    IEnumerator SendToLLM(string prompt)
    {
        Debug.Log(prompt);
        RequestData data = new RequestData(prompt);
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = new UnityWebRequest(URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                ResponseData res = JsonUtility.FromJson<ResponseData>(req.downloadHandler.text);
                response = res.choices[0].message.content;
            }
            else
            {
                response = "Error: " + req.error;
            }

            Repaint();
        }
    }

    IEnumerator SendRequest()
    {
        RequestData data = new RequestData(prompt);
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = new UnityWebRequest(URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");

            req.timeout = 120;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(req.downloadHandler.text);

                ResponseData res = JsonUtility.FromJson<ResponseData>(req.downloadHandler.text);

                if (res != null && res.choices != null && res.choices.Length > 0)
                {
                    response = res.choices[0].message.content;
                }
                else
                {
                    response = "Parse error!";
                }
            }
            else
            {
                response = "Error: " + req.error;
                Debug.LogError(req.error);
            }

            Repaint();
        }
    }

    // ===== DATA =====

    [System.Serializable]
    public class RequestData
    {
        public string model = "nvidia/nemotron-3-nano-4b";
        public Message[] messages;
        public float temperature = 0.7f;

        public RequestData()
        {
        }
        
        public RequestData(string userPrompt)
        {
            messages = new Message[]
            {
                new Message("system", "You are a helpful assistant"),
                new Message("user", userPrompt)
            };
        }
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;

        public Message()
        {
        }
        
        public Message(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [System.Serializable]
    public class ResponseData
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }
}