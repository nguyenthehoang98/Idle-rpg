using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
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
    private Vector2 scrollPos;
    private string selectedFile = "";
    private string fixInstruction = "";
    private Vector2 scrollFix;
    float fakeProgress = 0f;
    double lastTime;
    private MonoScript selectedScript = null;
    private EditorCoroutine coroutine;

    private const string URL = "http://127.0.0.1:1234/v1/chat/completions";

    [MenuItem("Tools/LMM/Review code")]
    public static void ShowWindow()
    {
        GetWindow<LLMReviewCodeEditorWindow>("Review code");
    }

    private string folderPath = "Assets/";

    void OnGUI()
    {
        GUILayout.Label("=== REVIEW FOLDER CODE ===", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Folder Path:", GUILayout.Width(100));
        folderPath = EditorGUILayout.TextField(folderPath);
        EditorGUILayout.EndHorizontal();

        GUI.enabled = !isRunning;
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Select Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                folderPath = path;
            }
        }

        if (GUILayout.Button("Analyze Files"))
        {
            if (coroutine != null) EditorCoroutineUtility.StopCoroutine(coroutine); 
            coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(AnalyzeFiles());
        }
        
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;
        
        if (isRunning)
        {
            double time = EditorApplication.timeSinceStartup;
            double delta = time - lastTime;
            lastTime = time;

            // tốc độ animation (tùy chỉnh)
            fakeProgress += (float)(delta * 0.5f);

            if (fakeProgress > 1f)
                fakeProgress = 0f;

            EditorGUILayout.HelpBox(
                $"Analyzing {currentFileIndex}/{totalFiles}\n{currentFileName}",
                MessageType.Info
            );

            Rect rect = GUILayoutUtility.GetRect(200, 20);
            EditorGUI.ProgressBar(rect, fakeProgress, "Processing...");
    
            Repaint(); // ⚠️ rất quan trọng để animate
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== REVIEW FIX CODE ===", EditorStyles.boldLabel);

        selectedScript = (MonoScript)EditorGUILayout.ObjectField(
            "C# File",
            selectedScript,
            typeof(MonoScript),
            false
        );

        if (selectedScript != null)
        {
            selectedFile = AssetDatabase.GetAssetPath(selectedScript);
        }

        fixInstruction = EditorGUILayout.TextField("Instruction:", fixInstruction);

        GUI.enabled = !isRunning;
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Review This File"))
        {
            if (coroutine != null) EditorCoroutineUtility.StopCoroutine(coroutine); 

            isRunning = true;
            response = string.Empty;
            totalFiles = 1;
            currentFileIndex = 0;
            
            coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(AnalyzeFile(selectedFile, () =>
            {
                currentFileIndex++;
                isRunning = false;
                currentFileName = "";
                response += "\nDone!";
            }));
        }
        GUI.enabled = true;
        
        GUI.enabled = !string.IsNullOrEmpty(fixInstruction) && !isRunning;
        
        if (GUILayout.Button("Fix This File"))
        {
            if (coroutine != null) EditorCoroutineUtility.StopCoroutine(coroutine); 
            
            isRunning = true;
            coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(FixFile(() =>
            {
                isRunning = false;
            }));
            
            Repaint();
        }
        
        GUI.enabled = true;
        
        GUI.enabled = !isRunning;
        
        if (GUILayout.Button("Apply Fix"))
        {
            System.IO.File.WriteAllText(selectedFile, response);
            AssetDatabase.Refresh();

            response += "\nApplied fix to: " + selectedFile;
        }
        
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        if (!isRunning)
        {
            var bugs = ExtractBugs(response);
            foreach (var bug in bugs)
            {
                if (GUILayout.Button("Issuie: " + bug))
                {
                    fixInstruction = "Fix this issue: " + bug;
                }
            }
        }
        
        if (isRunning)
        {
            if (GUILayout.Button("Stop"))
            {
                if(coroutine != null) EditorCoroutineUtility.StopCoroutine(coroutine);
            }
        }

        GUILayout.Label("Response:");
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(500));
        
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        style.richText = true;

        response = EditorGUILayout.TextArea(response, style, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
    }
    
    string ExtractBugBlock(string text)
    {
        int scoreIndex = text.IndexOf("[SCORE]");
        int perfIndex = text.IndexOf("[PERFORMANCE]");

        if (scoreIndex == -1 || perfIndex == -1 || perfIndex <= scoreIndex)
            return "";

        // lấy đoạn giữa SCORE và PERFORMANCE
        string block = text.Substring(scoreIndex, perfIndex - scoreIndex);

        // tìm vị trí [BUG] trong block
        int bugIndex = block.IndexOf("[BUG]");
        if (bugIndex == -1)
            return "";

        // chỉ lấy phần sau [BUG]
        return block.Substring(bugIndex + "[BUG]".Length);
    }
    
    List<string> ParseBugLines(string bugBlock)
    {
        List<string> bugs = new List<string>();

        var lines = bugBlock.Split('\n');

        foreach (var raw in lines)
        {
            string line = raw.Trim();

            if (line.StartsWith("-"))
            {
                bugs.Add(line.Substring(1).Trim());
            }
        }

        return bugs;
    }
    
    List<string> ExtractBugs(string text)
    {
        string bugBlock = ExtractBugBlock(text);
        return ParseBugLines(bugBlock);
    }
    
    IEnumerator FixFile(Action onComplete)
    {
        string code = System.IO.File.ReadAllText(selectedFile);

        string prompt =
            "You are a senior Unity developer.\n" +
            "Fix the following C# code.\n\n" +

            "Instruction:\n" + fixInstruction + "\n\n" +

            "Constraints:\n" +
            "- Keep original behavior unless fixing bug\n" +
            "- Do not remove important logic\n" +
            "- Keep code clean and readable\n\n" +

            "Output:\n" +
            "- Return FULL updated code only\n" +
            "- No explanation\n\n" +

            "Code:\n" + code;

        yield return SendFixRequest(prompt, onComplete);
        
        fixInstruction = String.Empty;
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
    
    IEnumerator SendFixRequest(string prompt, Action onComplete)
    {
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
                var res = JsonUtility.FromJson<ResponseData>(req.downloadHandler.text);

                response = res.choices[0].message.content;
            }
            else
            {
                response += "\nFix Error: " + req.error;
            }

            Repaint();
            onComplete?.Invoke();
        }
    }

    IEnumerator AnalyzeFile(string file, Action onComplete)
    {
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
        
        onComplete?.Invoke();
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
            yield return AnalyzeFile(file, null);
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

    [Serializable]
    class BugItem
    {
        public string file;
        public string message;
    }

    [Serializable]
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