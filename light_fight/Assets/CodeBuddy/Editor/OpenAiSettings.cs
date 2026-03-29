// Decompiled with JetBrains decompiler
// Type: CodeBuddy.OpenAiSettings
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
  internal class OpenAiSettings : AiSettings
  {
    [SerializeField]
    private string apiKey;
    private static StringEncryptor stringEncryptor = new StringEncryptor();
    private string unencryptedApiKey;
    [SerializeField]
    private string[] modelList = new string[0];

    public override IAiService Service => (IAiService) OpenAiResponsesService.Instance;

    public override string Name => "OpenAI";

    public override string[] ModelList => this.modelList;

    public string ApiKey
    {
      get
      {
        if (string.IsNullOrEmpty(this.unencryptedApiKey))
          this.unencryptedApiKey = OpenAiSettings.stringEncryptor.Decrypt(this.apiKey);
        return this.unencryptedApiKey;
      }
      set
      {
        if (value == this.unencryptedApiKey)
          return;
        this.unencryptedApiKey = value;
        string str = OpenAiSettings.stringEncryptor.Encrypt(value);
        if (this.apiKey == str)
          return;
        this.apiKey = str;
      }
    }

    public override void Reset()
    {
      base.Reset();
      this.ModelIndex = 0;
      this.BaseUrl = "https://api.openai.com/";
      this.CustomModelName = "pai-001-light-beta";
      this.UseCustomModel = false;
      this.Temperature = 0.01f;
      this.modelList = new string[0];
      this.apiKey = string.Empty;
      this.unencryptedApiKey = string.Empty;
    }

    public override async Task UpdateModelList()
    {
      this.modelList = OpenAiCompletionsService.Instance.GetModelList();
      await Task.CompletedTask;
    }
  }
}
