// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ClaudeSettings
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using CodeBuddy.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  internal class ClaudeSettings : AiSettings
  {
    [SerializeField]
    private string apiKey;
    private static StringEncryptor stringEncryptor = new StringEncryptor();
    private string unencryptedApiKey;
    [SerializeField]
    private string[] modelList = new string[0];
    [SerializeField]
    private int maxTokens = 4096;

    public override IAiService Service => (IAiService) ClaudeService.Instance;

    public override string Name => "Claude";

    public override string[] ModelList => this.modelList;

    public string ApiKey
    {
      get
      {
        if (string.IsNullOrEmpty(this.unencryptedApiKey))
          this.unencryptedApiKey = ClaudeSettings.stringEncryptor.Decrypt(this.apiKey);
        return this.unencryptedApiKey;
      }
      set
      {
        if (value == this.unencryptedApiKey)
          return;
        this.unencryptedApiKey = value;
        string str = ClaudeSettings.stringEncryptor.Encrypt(value);
        if (this.apiKey == str)
          return;
        this.apiKey = str;
        this.InvokeOnSettingsChanged(this.apiKey);
      }
    }

    public int MaxTokens
    {
      get => this.maxTokens;
      set
      {
        if (this.maxTokens == value)
          return;
        this.maxTokens = value;
        this.InvokeOnSettingsChanged(nameof (MaxTokens));
      }
    }

    public override void Reset()
    {
      base.Reset();
      this.ModelIndex = 0;
      this.apiKey = string.Empty;
      this.unencryptedApiKey = string.Empty;
      this.modelList = new string[0];
      this.BaseUrl = "https://api.anthropic.com/";
      this.CustomModelName = "claude-3-opus-20240229";
      this.UseCustomModel = false;
      this.Temperature = 0.01f;
      this.MaxTokens = 4096;
    }

    public override async Task UpdateModelList()
    {
      this.modelList = ClaudeService.Instance.GetModelList();
      this.InvokeOnSettingsChanged("ModelList");
      await Task.CompletedTask;
    }
  }
}
