// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GetPublicMembersTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.ComponentModel;
using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  public class GetPublicMembersTool : ToolBase<GetPublicMembersTool.GetPublicMembersParameters>
  {
    public GetPublicMembersTool()
      : base("GetPublicMembers", "Your PRIMARY tool for gathering information about a class. Use this FIRST to inspect a class's public fields, properties, and methods before generating code or calling other tools.")
    {
    }

    public override void Execute(
      string toolCallId,
      GetPublicMembersTool.GetPublicMembersParameters parameters)
    {
      string descriptionsWithComments = ScriptableSingleton<ProjectInfoGenerator>.instance.GetClassesWithComments();
      if (string.IsNullOrEmpty(descriptionsWithComments))
        this.InvokeExecutionFinished(toolCallId, "No public members found for the specified classes.");
      else
        this.InvokeExecutionFinished(toolCallId, descriptionsWithComments);
    }

    public class GetPublicMembersParameters : ToolParameters
    {
      [JsonProperty("classNames")]
      [Description("The list of class names to retrieve public members for. You must use exact class name for it.")]
      public string[] ClassNames;
    }
  }
}
