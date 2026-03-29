// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GetClassesWithCommentsTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  public class GetClassesWithCommentsTool : ToolBase<ToolParameters>
  {
    public GetClassesWithCommentsTool()
      : base("GetClassesWithComments", "Retrieves a list of classes with their comments.")
    {
    }

    public override void Execute(string toolCallId, ToolParameters parameters)
    {
      string classesWithComments = ScriptableSingleton<ProjectInfoGenerator>.instance.GetClassesWithComments();
      if (!string.IsNullOrEmpty(classesWithComments))
        this.InvokeExecutionFinished(toolCallId, classesWithComments);
      else
        this.InvokeExecutionFinished(toolCallId, "No classes with comments found in the project.");
    }
  }
}
