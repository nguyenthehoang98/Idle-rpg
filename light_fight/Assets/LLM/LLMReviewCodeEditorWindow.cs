using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Unity.EditorCoroutines.Editor;

public class LLMReviewCodeEditorWindow : EditorWindow
{
    private string prompt = "Say hello from Unity Editor";
    private string response = "";
    private bool isRunning = false;
    private int currentFileIndex = 0;
    private int totalFiles = 0;
    private string currentFileName = "";
    private float animTime = 0f;
    private Vector2 scrollPos;

    private const string URL = "http://127.0.0.1:1234/v1/chat/completions";

    [MenuItem("Tools/LMM/Review code")]
    public static void ShowWindow()
    {
        GetWindow<LLMReviewCodeEditorWindow>("Review code");
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

        GUI.enabled = !isRunning;

        if (GUILayout.Button("Analyze Files"))
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(AnalyzeFiles());
        }

        GUI.enabled = true;
        
        if (isRunning)
        {
            EditorGUILayout.HelpBox(
                $"Analyzing {currentFileIndex}/{totalFiles}\n{currentFileName}",
                MessageType.Info
            );

            float progress = (float)currentFileIndex / totalFiles;
            EditorGUI.ProgressBar(
                GUILayoutUtility.GetRect(200, 20),
                progress,
                $"{Mathf.RoundToInt(progress * 100)}%"
            );
        }
        
        if (isRunning)
        {
            animTime += 0.1f;
            int dots = (int)(animTime % 4);

            string loading = "Processing";
            for (int i = 0; i < dots; i++) loading += ".";

            GUILayout.Label(loading);
        }

        GUILayout.Label("Response:");
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(500));
        
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        style.richText = true;

        response = EditorGUILayout.TextArea(response, style, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
    }
    
    string FormatRichText(string input)
    {
        input = input.Replace("[BUG]", "<color=red>[BUG]</color>");
        input = input.Replace("[GOOD]", "<color=green>[GOOD]</color>");
        input = input.Replace("[PERFORMANCE]", "<color=orange>[PERFORMANCE]</color>");
        input = input.Replace("[ARCHITECTURE]", "<color=cyan>[ARCHITECTURE]</color>");
        input = input.Replace("[SUGGESTIONS]", "<color=yellow>[SUGGESTIONS]</color>");
        input = input.Replace("[SCORE]", "<color=lime>[SCORE]</color>");

        return input;
    }
    
    IEnumerator AnalyzeFiles()
    {
        string[] files = System.IO.Directory.GetFiles(folderPath, "*.cs", System.IO.SearchOption.AllDirectories);
        isRunning = true;
        response = string.Empty;
        totalFiles = files.Length;
        currentFileIndex = 0;
        
        foreach (var file in files)
        {
            currentFileIndex++;
            currentFileName = file;
            
            string code = System.IO.File.ReadAllText(file);

            // limit mỗi file để tránh overload
            if (code.Length > 4000)
                code = code.Substring(0, 4000);

            string prompt =
                "You are a senior Unity developer.\n" +
                "Review this C# file.\n\n" +

                "Requirements:\n" +
                "- DO NOT return JSON\n" +
                "- Write like a debug report\n" +
                "- Be concise\n" +
                "- Use simple Vietnamese\n\n" +

                "Scoring:\n" +
                "- Overall score: X/10\n\n" +

                "Format:\n" +
                "[FILE]: " + file + "\n" +
                "[SCORE]: X/10\n\n" +

                "[BUG]\n- ...\n\n" +
                "[PERFORMANCE]\n- ...\n\n" +
                "[ARCHITECTURE]\n- ...\n\n" +
                "[GOOD]\n- ...\n\n" +
                "[SUGGESTIONS]\n- ...\n\n" +

                "Important:\n" +
                "- Prefix issues with [BUG]\n" +
                "- Prefix good parts with [GOOD]\n" +
                "- Keep it short\n\n" +

                "Code:\n" + code;

            yield return SendToLLMAppend(prompt);

            response += "\n----------------------------------------\n";
            Repaint();
        }

        isRunning = false;
        currentFileName = "";
        response += "\nDone!";
    }
    
    IEnumerator SendToLLMAppend(string prompt)
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
                var res = JsonUtility.FromJson<ResponseData>(req.downloadHandler.text);

                if (res != null && res.choices != null && res.choices.Length > 0)
                {
                    response += FormatRichText(res.choices[0].message.content) + "\n";
                }
                else
                {
                    response += "Parse error\n";
                }
            }
            else
            {
                response += "Error: " + req.error + "\n";
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