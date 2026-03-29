// Decompiled with JetBrains decompiler
// Type: CodeBuddy.DeepSeekSettings
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using CodeBuddy.Services;
using System;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  internal class DeepSeekSettings : AiSettings
  {
    [SerializeField]
    private string apiKey;
    private static StringEncryptor stringEncryptor = new StringEncryptor();
    private string unencryptedApiKey;

    public override IAiService Service => (IAiService) DeepSeekService.Instance;

    public override string Name => "DeepSeek";

    public override string[] ModelList
    {
      get
      {
        return new string[2]
        {
          "deepseek-chat",
          "deepseek-reasoner"
        };
      }
    }

    public string ApiKey
    {
      get
      {
        if (string.IsNullOrEmpty(this.unencryptedApiKey))
          this.unencryptedApiKey = DeepSeekSettings.stringEncryptor.Decrypt(this.apiKey);
        return this.unencryptedApiKey;
      }
      set
      {
        if (value == this.unencryptedApiKey)
          return;
        this.unencryptedApiKey = value;
        string str = DeepSeekSettings.stringEncryptor.Encrypt(value);
        if (this.apiKey == str)
          return;
        this.apiKey = str;
      }
    }

    public override void Reset()
    {
      base.Reset();
      this.ModelIndex = 0;
      this.BaseUrl = "https://api.deepseek.com/";
      this.CustomModelName = "deepseek-chat";
      this.UseCustomModel = false;
      this.Temperature = 0.01f;
      this.apiKey = string.Empty;
      this.unencryptedApiKey = string.Empty;
    }
  }
}
