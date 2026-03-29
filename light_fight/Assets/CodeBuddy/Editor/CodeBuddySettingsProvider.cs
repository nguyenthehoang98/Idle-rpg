// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeBuddySettingsProvider
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  internal class CodeBuddySettingsProvider : SettingsProvider
  {
    private Settings _settings;
    private CodeBuddySettingsProvider.ConnectionStatus _connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Neutral;

    public CodeBuddySettingsProvider(string path, SettingsScope scope = SettingsScope.User)
      : base(path, scope)
    {
      this._settings = ScriptableSingleton<Settings>.instance;
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
      return (SettingsProvider) new CodeBuddySettingsProvider("Project/Code Buddy", SettingsScope.Project);
    }

    private void MakeOpenAiCompletionsSettings(OpenAiSettings completionsSettings)
    {
      EditorGUI.BeginChangeCheck();
      completionsSettings.ApiKey = EditorGUILayout.TextField("OpenAI API Key:", completionsSettings.ApiKey);
      completionsSettings.BaseUrl = EditorGUILayout.TextField("Base URL:", completionsSettings.BaseUrl);
      if (EditorGUI.EndChangeCheck())
        this._settings.Service.Reset();
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Test Connection"))
        this.TestConnection();
      switch (this._settings.Service.Status)
      {
        case ServiceStatus.Active:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Success;
          break;
        case ServiceStatus.Failed:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Failure;
          break;
        default:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Neutral;
          break;
      }
      GUI.color = this.GetConnectionStatusColor(this._connectionStatus);
      GUILayout.Label("●", GUILayout.Width(20f));
      GUI.color = Color.white;
      GUILayout.EndHorizontal();
      if (string.IsNullOrEmpty(completionsSettings.ApiKey))
        return;
      this.MakeModelSelector((AiSettings) completionsSettings, true);
    }

    private void MakeModelSelector(AiSettings aiSettings, bool showUpdateModelButton = false)
    {
      aiSettings.UseCustomModel = EditorGUILayout.Toggle("Use Custom Model", aiSettings.UseCustomModel);
      if (!aiSettings.UseCustomModel)
      {
        if (showUpdateModelButton)
        {
          EditorGUILayout.BeginHorizontal();
          if ((aiSettings.ModelList == null || aiSettings.ModelList.Length == 0) && this._connectionStatus == CodeBuddySettingsProvider.ConnectionStatus.Success)
            aiSettings.UpdateModelList().GetAwaiter().GetResult();
          aiSettings.ModelIndex = EditorGUILayout.Popup("Model:", aiSettings.ModelIndex, aiSettings.ModelList);
          GUIContent content = EditorGUIUtility.IconContent("d_RotateTool");
          content.tooltip = "Refresh model list";
          if (GUILayout.Button(content, GUILayout.Width(23f), GUILayout.Height(18f)))
          {
            aiSettings.UpdateModelList().GetAwaiter().GetResult();
            this.TestConnection();
          }
          EditorGUILayout.EndHorizontal();
        }
        else
          aiSettings.ModelIndex = EditorGUILayout.Popup("Model:", aiSettings.ModelIndex, aiSettings.ModelList);
      }
      else
        aiSettings.CustomModelName = EditorGUILayout.TextField("Model:", aiSettings.CustomModelName);
    }

    private void MakeFolderListSettings(string label, List<string> folders)
    {
      EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
      for (int index = 0; index < folders.Count; ++index)
      {
        GUILayout.BeginHorizontal();
        Rect rect = GUILayoutUtility.GetRect(new GUIContent(folders[index]), GUI.skin.textField);
        EditorGUI.BeginChangeCheck();
        folders[index] = EditorGUI.TextField(rect, folders[index]);
        if (EditorGUI.EndChangeCheck())
          this._settings.Save();
        if (GUILayout.Button("-", GUILayout.Width(60f)))
        {
          folders.RemoveAt(index);
          --index;
          this._settings.Save();
        }
        GUILayout.EndHorizontal();
        if ((UnityEngine.Event.current.type == UnityEngine.EventType.DragUpdated || UnityEngine.Event.current.type == UnityEngine.EventType.DragPerform) && rect.Contains(UnityEngine.Event.current.mousePosition))
        {
          DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
          if (UnityEngine.Event.current.type == UnityEngine.EventType.DragPerform)
          {
            DragAndDrop.AcceptDrag();
            foreach (string path in DragAndDrop.paths)
            {
              if (Directory.Exists(path))
              {
                folders[index] = path;
                this._settings.Save();
              }
            }
          }
          UnityEngine.Event.current.Use();
        }
      }
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button("+", GUILayout.Width(60f)))
      {
        string str = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
        if (!string.IsNullOrEmpty(str))
        {
          if (str.StartsWith(Application.dataPath))
            str = "Assets" + str.Substring(Application.dataPath.Length);
          folders.Add(str);
          this._settings.Save();
        }
      }
      GUILayout.EndHorizontal();
    }

    private void MakeExcludedFoldersSettings()
    {
      this.MakeFolderListSettings("Excluded Folders", this._settings.ExcludedFolders);
    }

    private void MakeIncludeFoldersSettings()
    {
      this.MakeFolderListSettings("Included Folders", this._settings.IncludedFolders);
    }

    private void MakeGeminiSettings(GeminiSettings geminiSettings)
    {
      EditorGUI.BeginChangeCheck();
      geminiSettings.ApiKey = EditorGUILayout.TextField("Gemini API Key:", geminiSettings.ApiKey);
      geminiSettings.BaseUrl = EditorGUILayout.TextField("Base URL:", geminiSettings.BaseUrl);
      if (EditorGUI.EndChangeCheck())
        this._settings.Service.Reset();
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Test Connection"))
        this.TestConnection();
      switch (this._settings.Service.Status)
      {
        case ServiceStatus.Active:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Success;
          break;
        case ServiceStatus.Failed:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Failure;
          break;
        default:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Neutral;
          break;
      }
      GUI.color = this.GetConnectionStatusColor(this._connectionStatus);
      GUILayout.Label("●", GUILayout.Width(20f));
      GUI.color = Color.white;
      GUILayout.EndHorizontal();
      if (string.IsNullOrEmpty(geminiSettings.ApiKey))
        return;
      this.MakeModelSelector((AiSettings) geminiSettings, true);
    }

    public override void OnGUI(string searchContext)
    {
      this._settings.SelectedServiceIndex = EditorGUILayout.Popup("Provider:", this._settings.SelectedServiceIndex, this._settings.ServiceNames);
      GUILayout.Space(10f);
      GUILayout.BeginVertical();
      EditorGUILayout.Separator();
      if (this._settings.ServiceSettings is OpenAiSettings serviceSettings5)
        this.MakeOpenAiCompletionsSettings(serviceSettings5);
      else if (this._settings.ServiceSettings is OllamaSettings serviceSettings4)
        this.MakeOllamaSettings(serviceSettings4);
      else if (this._settings.ServiceSettings is DeepSeekSettings serviceSettings3)
        this.MakeDeepSeekSettings(serviceSettings3);
      else if (this._settings.ServiceSettings is GeminiSettings serviceSettings2)
        this.MakeGeminiSettings(serviceSettings2);
      else if (this._settings.ServiceSettings is ClaudeSettings serviceSettings1)
        this.MakeClaudeSettings(serviceSettings1);
      GUILayout.Space(5f);
      this._settings.ServiceSettings.Temperature = EditorGUILayout.Slider("Temperature", this._settings.ServiceSettings.Temperature, 0.0f, 1f);
      GUILayout.Space(5f);
      GUILayout.Label("Instructions:");
      this._settings.Instructions = GUILayout.TextArea(this._settings.Instructions, GUILayout.ExpandWidth(false));
      EditorGUILayout.Separator();
      GUILayout.EndVertical();
      GUILayout.Space(10f);
      this._settings.IncludeEditorFolder = EditorGUILayout.Toggle("Include Editor Folders", this._settings.IncludeEditorFolder);
      GUILayout.Space(10f);
      this._settings.UseWhitelist = EditorGUILayout.Toggle("Use Whitelist Folders", this._settings.UseWhitelist);
      if (this._settings.UseWhitelist)
        this.MakeIncludeFoldersSettings();
      else
        this.MakeExcludedFoldersSettings();
      this._settings.AskWhereToSave = EditorGUILayout.Toggle("Ask Where To Save", this._settings.AskWhereToSave);
      if (!this._settings.AskWhereToSave)
        this._settings.SaveFolderName = EditorGUILayout.TextField("Default Scripts Folder:", this._settings.SaveFolderName);
      this._settings.SkipDiffReview = EditorGUILayout.Toggle("Skip Diff Review", this._settings.SkipDiffReview);
      GUILayout.Space(10f);
      if (GUILayout.Button("Reinitialize project cache"))
        ScriptableSingleton<ProjectInfoGenerator>.instance.InitializeCache();
      GUILayout.Space(20f);
      if (GUILayout.Button("Reset"))
      {
        this._settings.ResetSettings();
        this._settings.Service.Reset();
      }
      GUILayout.Space(10f);
      if (!GUILayout.Button("Clear History"))
        return;
      this.ClearHistory();
    }

    private void MakeOllamaSettings(OllamaSettings ollamaSettings)
    {
      EditorGUI.BeginChangeCheck();
      ollamaSettings.BaseUrl = EditorGUILayout.TextField("Base URL:", ollamaSettings.BaseUrl);
      if (GUILayout.Button("Refresh"))
      {
        ollamaSettings.UpdateModelList().GetAwaiter().GetResult();
        this.TestConnection();
      }
      if (EditorGUI.EndChangeCheck())
        this._settings.Service.Reset();
      this.MakeModelSelector((AiSettings) ollamaSettings);
      ollamaSettings.ContextSize = EditorGUILayout.IntField("Context Size:", ollamaSettings.ContextSize);
    }

    private void MakeDeepSeekSettings(DeepSeekSettings deepSeekSettings)
    {
      EditorGUI.BeginChangeCheck();
      deepSeekSettings.ApiKey = EditorGUILayout.TextField("DeepSeek API Key:", deepSeekSettings.ApiKey);
      deepSeekSettings.BaseUrl = EditorGUILayout.TextField("Base URL:", deepSeekSettings.BaseUrl);
      if (EditorGUI.EndChangeCheck())
        this._settings.Service.Reset();
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Test Connection"))
        this.TestConnection();
      switch (this._settings.Service.Status)
      {
        case ServiceStatus.Active:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Success;
          break;
        case ServiceStatus.Failed:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Failure;
          break;
        default:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Neutral;
          break;
      }
      GUI.color = this.GetConnectionStatusColor(this._connectionStatus);
      GUILayout.Label("●", GUILayout.Width(20f));
      GUI.color = Color.white;
      GUILayout.EndHorizontal();
    }

    private void MakeClaudeSettings(ClaudeSettings claudeSettings)
    {
      EditorGUI.BeginChangeCheck();
      claudeSettings.ApiKey = EditorGUILayout.TextField("Claude API Key:", claudeSettings.ApiKey);
      claudeSettings.BaseUrl = EditorGUILayout.TextField("Base URL:", claudeSettings.BaseUrl);
      if (EditorGUI.EndChangeCheck())
        this._settings.Service.Reset();
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Test Connection"))
        this.TestConnection();
      switch (this._settings.Service.Status)
      {
        case ServiceStatus.Active:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Success;
          break;
        case ServiceStatus.Failed:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Failure;
          break;
        default:
          this._connectionStatus = CodeBuddySettingsProvider.ConnectionStatus.Neutral;
          break;
      }
      GUI.color = this.GetConnectionStatusColor(this._connectionStatus);
      GUILayout.Label("●", GUILayout.Width(20f));
      GUI.color = Color.white;
      GUILayout.EndHorizontal();
      if (!string.IsNullOrEmpty(claudeSettings.ApiKey))
        this.MakeModelSelector((AiSettings) claudeSettings, true);
      claudeSettings.MaxTokens = EditorGUILayout.IntField("Max Tokens:", claudeSettings.MaxTokens);
    }

    private void TestConnection() => this._settings.Service.Reset();

    private Color GetConnectionStatusColor(CodeBuddySettingsProvider.ConnectionStatus status)
    {
      switch (status)
      {
        case CodeBuddySettingsProvider.ConnectionStatus.Success:
          return Color.green;
        case CodeBuddySettingsProvider.ConnectionStatus.Failure:
          return Color.red;
        default:
          return Color.gray;
      }
    }

    private void ClearHistory()
    {
      CodeBuddyHistory instance = ScriptableSingleton<CodeBuddyHistory>.instance;
      instance.OpenAiCompletionHistory.Clear();
      instance.OllamaHistory.Clear();
      instance.GeminiHistory.Clear();
      instance.ClaudeHistory.Clear();
      instance.Save();
      this._settings.Service.Reset();
    }

    private enum ConnectionStatus
    {
      Neutral,
      Success,
      Failure,
    }
  }
}
