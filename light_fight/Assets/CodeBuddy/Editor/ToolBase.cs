// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ToolBase
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  public abstract class ToolBase
  {
    public readonly string name;
    public readonly string description;

    public event Action<string, string, bool> ExecutionFinished;

    protected void InvokeExecutionFinished(
      string toolCallId,
      string result,
      bool invokeAfterReload = false)
    {
      Action<string, string, bool> executionFinished = this.ExecutionFinished;
      if (executionFinished == null)
        return;
      executionFinished(toolCallId, result, invokeAfterReload);
    }

    public ToolBase(string _name, string _description)
    {
      this.name = _name;
      this.description = _description;
    }
  }
}
