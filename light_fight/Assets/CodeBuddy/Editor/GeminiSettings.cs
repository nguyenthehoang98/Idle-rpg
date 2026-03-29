// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GeminiSettings
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
  internal class GeminiSettings : AiSettings
  {
    [SerializeField]
    private string apiKey;
    private static StringEncryptor stringEncryptor = new StringEncryptor();
    private string unencryptedApiKey;
    [SerializeField]
    private string[] modelList = new string[0];

    public override IAiService Service => (IAiService) GeminiService.Instance;

    public override string Name => "Gemini";

    public override string[] ModelList => this.modelList;

    public string ApiKey
    {
      get
      {
        if (string.IsNullOrEmpty(this.unencryptedApiKey))
          this.unencryptedApiKey = GeminiSettings.stringEncryptor.Decrypt(this.apiKey);
        return this.unencryptedApiKey;
      }
      set
      {
        if (value == this.unencryptedApiKey)
          return;
        this.unencryptedApiKey = value;
        string str = GeminiSettings.stringEncryptor.Encrypt(value);
        if (this.apiKey == str)
          return;
        this.apiKey = str;
        this.InvokeOnSettingsChanged("apiKey");
      }
    }

    public override void Reset()
    {
      base.Reset();
      this.apiKey = string.Empty;
      this.unencryptedApiKey = string.Empty;
      this.modelList = new string[0];
      this.ModelIndex = 0;
      this.BaseUrl = "https://generativelanguage.googleapis.com/";
      this.CustomModelName = "gemini-2.0-flash";
      this.UseCustomModel = false;
      this.Temperature = 0.01f;
    }

    public override async Task UpdateModelList()
    {
      this.modelList = GeminiService.Instance.GetModelList();
      this.InvokeOnSettingsChanged("ModelList");
      await Task.CompletedTask;
    }
  }
}
