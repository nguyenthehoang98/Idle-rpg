// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Settings
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  [FilePath("ProjectSettings/CodeBuddySettings.asset", FilePathAttribute.Location.ProjectFolder)]
  internal class Settings : ScriptableSingleton<Settings>
  {
    [SerializeField]
    private OpenAiSettings aiCompletionsSettings = new OpenAiSettings();
    [SerializeField]
    private OllamaSettings ollamaSettings = new OllamaSettings();
    [SerializeField]
    private DeepSeekSettings deepSeekSettings = new DeepSeekSettings();
    [SerializeField]
    private GeminiSettings geminiSettings = new GeminiSettings();
    [SerializeField]
    private ClaudeSettings claudeSettings = new ClaudeSettings();
    private List<AiSettings> aiSettingsList;
    [SerializeField]
    private int selectedServiceIndex = 0;
    [SerializeField]
    private bool includeEditorFolder;
    [SerializeField]
    private bool askWhereToSave = false;
    [SerializeField]
    private bool skipDiffReview = false;
    [SerializeField]
    private string saveFolderName = "Scripts";
    [SerializeField]
    private bool useWhiteList = false;
    [SerializeField]
    private List<string> excludedFolders = new List<string>();
    [SerializeField]
    private List<string> includedFolders = new List<string>();
    [SerializeField]
    private string instructions = "";

    public IAiService Service
    {
      get
      {
        if (this.aiSettingsList == null)
          this.InitSettings();
        return this.aiSettingsList[this.selectedServiceIndex].Service;
      }
    }

    public AiSettings ServiceSettings
    {
      get
      {
        if (this.aiSettingsList == null)
          this.InitSettings();
        return this.aiSettingsList[this.selectedServiceIndex];
      }
    }

    public OpenAiSettings OpenAiCompletionsSettings => this.aiCompletionsSettings;

    public OllamaSettings OllamaSettings => this.ollamaSettings;

    public DeepSeekSettings DeepSeekSettings => this.deepSeekSettings;

    public GeminiSettings GeminiSettings => this.geminiSettings;

    public ClaudeSettings ClaudeSettings => this.claudeSettings;

    public string[] ServiceNames
    {
      get
      {
        if (this.aiSettingsList == null)
          this.InitSettings();
        return this.aiSettingsList.Select<AiSettings, string>((Func<AiSettings, string>) (x => x.Name)).ToArray<string>();
      }
    }

    public string Instructions
    {
      get => this.instructions;
      set
      {
        if (value == this.instructions)
          return;
        this.instructions = value;
        this.Save();
      }
    }

    public bool SkipDiffReview
    {
      get => this.skipDiffReview;
      set
      {
        if (value == this.skipDiffReview)
          return;
        this.skipDiffReview = value;
        this.Save();
      }
    }

    public bool IncludeEditorFolder
    {
      get => this.includeEditorFolder;
      set
      {
        if (value == this.includeEditorFolder)
          return;
        this.includeEditorFolder = value;
        this.Save();
      }
    }

    public bool AskWhereToSave
    {
      get => this.askWhereToSave;
      set
      {
        if (value == this.askWhereToSave)
          return;
        this.askWhereToSave = value;
        this.Save();
      }
    }

    public string SaveFolderName
    {
      get => this.saveFolderName;
      set
      {
        if (value == this.saveFolderName)
          return;
        this.saveFolderName = value;
        this.Save();
      }
    }

    public bool UseWhitelist
    {
      get => this.useWhiteList;
      set
      {
        if (value == this.useWhiteList)
          return;
        this.useWhiteList = value;
        this.Save();
      }
    }

    public List<string> IncludedFolders
    {
      get => this.includedFolders;
      set
      {
        if (value == this.includedFolders)
          return;
        this.includedFolders = value;
        this.Save();
      }
    }

    public List<string> ExcludedFolders
    {
      get => this.excludedFolders;
      set
      {
        if (value == this.excludedFolders)
          return;
        this.excludedFolders = value;
        this.Save();
      }
    }

    public int SelectedServiceIndex
    {
      get => this.selectedServiceIndex;
      set
      {
        if (value == this.selectedServiceIndex)
          return;
        this.selectedServiceIndex = value;
        this.Save();
        CodeBuddyMessageBus.Instance.Publish<MAiServiceChanged>(new MAiServiceChanged());
      }
    }

    public void ResetSettings()
    {
      this.OnDisable();
      foreach (AiSettings aiSettings in this.aiSettingsList)
        aiSettings.Reset();
      this.OnEnable();
      this.includeEditorFolder = false;
      this.askWhereToSave = false;
      this.saveFolderName = "Scripts";
      this.excludedFolders.Clear();
      this.includedFolders.Clear();
      this.useWhiteList = false;
      this.LoadDefaultInstructions();
      this.Save();
      ScriptableSingleton<CodeBuddyHistory>.instance.OpenAiCompletionHistory.Clear();
      ScriptableSingleton<CodeBuddyHistory>.instance.OllamaHistory.Clear();
      ScriptableSingleton<CodeBuddyHistory>.instance.DeepSeekHistory.Clear();
      ScriptableSingleton<CodeBuddyHistory>.instance.GeminiHistory.Clear();
      ScriptableSingleton<CodeBuddyHistory>.instance.ClaudeHistory.Clear();
      ScriptableSingleton<CodeBuddyHistory>.instance.Save();
    }

    public void Save() => this.Save(true);

    private void OnEnable()
    {
      this.InitSettings();
      foreach (AiSettings aiSettings in this.aiSettingsList)
        aiSettings.OnSettingsChanged += new Action<string>(this.ServiceSettings_OnSettingsChanged);
    }

    private void InitSettings()
    {
      this.aiSettingsList = new List<AiSettings>()
      {
        (AiSettings) this.aiCompletionsSettings,
        (AiSettings) this.ollamaSettings,
        (AiSettings) this.deepSeekSettings,
        (AiSettings) this.geminiSettings,
        (AiSettings) this.claudeSettings
      };
      if (!string.IsNullOrEmpty(this.instructions))
        return;
      this.LoadDefaultInstructions();
    }

    private void LoadDefaultInstructions()
    {
      this.Instructions = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath("ecf9c73687d760047aae9d7bc0feedea")).text;
      this.Save();
    }

    private void OnDisable()
    {
      foreach (AiSettings aiSettings in this.aiSettingsList)
        aiSettings.OnSettingsChanged -= new Action<string>(this.ServiceSettings_OnSettingsChanged);
    }

    private void ServiceSettings_OnSettingsChanged(string propName) => this.Save();
  }
}
