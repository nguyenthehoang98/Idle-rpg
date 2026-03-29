// Decompiled with JetBrains decompiler
// Type: CodeBuddy.OllamaSettings
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
  internal class OllamaSettings : AiSettings
  {
    [SerializeField]
    private int contextSize;
    [SerializeField]
    private string[] modelList = new string[0];

    public override IAiService Service => (IAiService) OllamaService.Instance;

    public override string Name => "Ollama";

    public override string[] ModelList => this.modelList;

    public int ContextSize
    {
      get => this.contextSize;
      set
      {
        if (value == this.contextSize)
          return;
        this.contextSize = value;
        this.InvokeOnSettingsChanged(nameof (ContextSize));
      }
    }

    public override void Reset()
    {
      base.Reset();
      this.BaseUrl = "http://localhost:11434/api/";
      this.ModelIndex = 0;
      this.UseCustomModel = false;
      this.CustomModelName = string.Empty;
      this.modelList = new string[0];
      this.Temperature = 0.01f;
      this.contextSize = 2048;
    }

    public override async Task UpdateModelList()
    {
      this.modelList = OllamaService.Instance.GetModelList();
      await Task.CompletedTask;
    }
  }
}
