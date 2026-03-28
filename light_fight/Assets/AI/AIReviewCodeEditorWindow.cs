using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AI;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class AIReviewCodeEditorWindow : AIBash
{
    private const string DEFAULT_FOLDER = "Assets/";

    #region State

    private string folderPath = DEFAULT_FOLDER;
    private string response = string.Empty;

    private bool isRunning;
    private int currentFileIndex;
    private int totalFiles;
    private string currentFileName;

    private float fakeProgress;
    private double fakeProgressLastTime;

    private double currentTime;
    private double runningLastTime;

    private Vector2 scrollPos;

    private MonoScript selectedScript;
    private string selectedFile;

    private EditorCoroutine coroutine;

    #endregion

    [MenuItem("Tools/AI/Review code")]
    public static void ShowWindow()
    {
        GetWindow<AIReviewCodeEditorWindow>("Review code");
    }

    #region GUI

    private void OnGUI()
    {
        DrawFolderSection();
        DrawFileSection();
        DrawProgress();

        if (isRunning && GUILayout.Button("Stop"))
            StopProcess();
        DrawResponse();
    }

    private void DrawFolderSection()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Folder Path:", GUILayout.Width(100));
        folderPath = EditorGUILayout.TextField(folderPath);
        EditorGUILayout.EndHorizontal();

        GUI.enabled = !isRunning;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Select Folder"))
        {
            var path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path)) folderPath = path;
        }

        if (GUILayout.Button("Analyze Files"))
        {
            StartCoroutine(AnalyzeFiles());
        }

        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
    }

    private void DrawProgress()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Last time running: " + TimeSpan.FromSeconds(currentTime - runningLastTime));
        
        if (!isRunning) return;

        currentTime= EditorApplication.timeSinceStartup;
        
        UpdateFakeProgress();

        EditorGUILayout.HelpBox(
            $"Analyzing {currentFileIndex}/{totalFiles}\n{currentFileName}",
            MessageType.Info
        );

        var rect = GUILayoutUtility.GetRect(200, 20);
        EditorGUI.ProgressBar(rect, fakeProgress, "Processing...");

        Repaint();
    }

    private void DrawFileSection()
    {
        GUILayout.Space(10);

        selectedScript = (MonoScript)EditorGUILayout.ObjectField(
            "C# File",
            selectedScript,
            typeof(MonoScript),
            false
        );

        if (selectedScript != null)
            selectedFile = AssetDatabase.GetAssetPath(selectedScript);

        GUI.enabled = selectedScript != null && !isRunning;

        if (GUILayout.Button("Review This File"))
            StartSingleFileReview();

        GUI.enabled = true;
    }

    private void DrawResponse()
    {
        GUILayout.Label("Response:");

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(Screen.height - 200));

        var style = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            richText = true
        };

        response = EditorGUILayout.TextArea(response, style, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Actions

    private void StartCoroutine(IEnumerator routine)
    {
        StopCurrentCoroutine();
        coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(routine);
    }

    private void StopCurrentCoroutine()
    {
        if (coroutine != null)
            EditorCoroutineUtility.StopCoroutine(coroutine);
    }

    private void StopProcess()
    {
        isRunning = false;
        StopCurrentCoroutine();
        Repaint();
    }

    private void StartSingleFileReview()
    {
        isRunning = true;
        runningLastTime = EditorApplication.timeSinceStartup;
        response = string.Empty;
        totalFiles = 1;
        currentFileIndex = 0;

        StartCoroutine(AnalyzeFile(selectedFile, () =>
        {
            currentFileIndex++;
            isRunning = false;
            currentFileName = string.Empty;
            response += "\nDone!";
        }));
    }

    #endregion

    #region Core Logic

    private IEnumerator AnalyzeFiles()
    {
        var files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

        isRunning = true;
        runningLastTime = EditorApplication.timeSinceStartup;
        response = string.Empty;
        totalFiles = files.Length;
        currentFileIndex = 0;

        foreach (var file in files)
        {
            if(File.Exists(file))
            {
                currentFileName = file;
                currentFileIndex++;
                yield return AnalyzeFile(file, null);
            }
            Repaint();
        }

        isRunning = false;
        currentFileName = string.Empty;
        response += "\nDone!";
    }

    private IEnumerator AnalyzeFile(string file, Action onComplete)
    {
        var code = File.ReadAllText(file);
        if (code.Length > 4000)
            code = code.Substring(0, 4000);

        var prompt = BuildReviewPrompt(file, code);

        yield return SendRequest(prompt, tuple =>
        {
            if (tuple.success)
            {
                response += FormatRichText(tuple.response) + "\n";
            }
            else
            {
                response += tuple.response + "\n";
            }
        });

        response += "\n\n";
        onComplete?.Invoke();
    }

    #endregion

    #region Prompt Builders

    private string BuildReviewPrompt(string file, string code)
    {
        return $@"You are a senior Unity developer.
Review this C# file.

Requirements:
- DO NOT return JSON
- Write like a debug report
- Use simple Vietnamese

Format:
[FILE]: {file}
[SCORE]: X/10

[BUG]
- ...

[PERFORMANCE]
- ...

[ARCHITECTURE]
- ...

[GOOD]
- ...

[SUGGESTIONS]
- ...

Code:
{code}";
    }

    #endregion

    #region Helpers

    private void UpdateFakeProgress()
    {
        var time = EditorApplication.timeSinceStartup;
        var delta = time - fakeProgressLastTime;
        fakeProgressLastTime = time;

        fakeProgress += (float)(delta * 0.5f);
        if (fakeProgress > 1f) fakeProgress = 0f;
    }

    private string FormatRichText(string input)
    {
        return input
            .Replace("[FILE]", "<color=magenta>[FILE]</color>")
            .Replace("[BUG]", "<color=red>[BUG]</color>")
            .Replace("[GOOD]", "<color=green>[GOOD]</color>")
            .Replace("[PERFORMANCE]", "<color=orange>[PERFORMANCE]</color>")
            .Replace("[ARCHITECTURE]", "<color=cyan>[ARCHITECTURE]</color>")
            .Replace("[SUGGESTIONS]", "<color=yellow>[SUGGESTIONS]</color>")
            .Replace("[SCORE]", "<color=lime>[SCORE]</color>");
    }

    #endregion

    protected override string Model => "qwen2.5-coder:7b";
}
