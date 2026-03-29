// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ToolParameters
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.ComponentModel;

#nullable enable
namespace CodeBuddy
{
  public class ToolParameters
  {
    [JsonProperty("userMessage", Required = Required.Always)]
    [Description("Shortest possible message to user to inform what is going to happen.")]
    public string UserMessage;
  }
}
