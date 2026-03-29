// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeBuddyHistory
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  [FilePath("ProjectSettings/CodeBuddyHistory.asset", FilePathAttribute.Location.ProjectFolder)]
  internal class CodeBuddyHistory : ScriptableSingleton<CodeBuddyHistory>
  {
    [SerializeField]
    private List<ServiceChat> openAiCompletionsHistory = new List<ServiceChat>();
    [SerializeField]
    private List<ServiceChat> ollamaHistory = new List<ServiceChat>();
    [SerializeField]
    private List<ServiceChat> deepseekHistory = new List<ServiceChat>();
    [SerializeField]
    private List<ServiceChat> geminiHistory = new List<ServiceChat>();
    [SerializeField]
    private List<ServiceChat> claudeHistory = new List<ServiceChat>();

    public List<ServiceChat> OpenAiCompletionHistory => this.openAiCompletionsHistory;

    public List<ServiceChat> OllamaHistory => this.ollamaHistory;

    public List<ServiceChat> DeepSeekHistory => this.deepseekHistory;

    public List<ServiceChat> GeminiHistory => this.geminiHistory;

    public List<ServiceChat> ClaudeHistory => this.claudeHistory;

    public void Save() => this.Save(true);
  }
}
