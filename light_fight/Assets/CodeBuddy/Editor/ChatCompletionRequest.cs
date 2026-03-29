// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ChatCompletionRequest
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  internal class ChatCompletionRequest
  {
    public CompletionMessageBase[] messages;
    public string model;
    public float temperature = 0.01f;
    public bool stream;
    public object[] tools;
    public bool parallel_tool_calls = false;
  }
}
