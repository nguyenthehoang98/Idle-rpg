// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ApplyChangesToSourceCodeTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  public class ApplyChangesToSourceCodeTool : 
    ToolBase<ApplyChangesToSourceCodeTool.ApplyChangesParameters>
  {
    public ApplyChangesToSourceCodeTool()
      : base("apply_changes", "DANGER: Use this tool ONLY for modifying the internal logic of an existing C# file. Changes MUST be correct before applying.")
    {
    }

    public override void Execute(
      string toolCallId,
      ApplyChangesToSourceCodeTool.ApplyChangesParameters parameters)
    {
      string filePath = ScriptableSingleton<ProjectInfoGenerator>.instance.GetFilePathForClass(parameters.ClassName);
      if (filePath == null)
        this.InvokeExecutionFinished(toolCallId, parameters.ClassName + " sources not found");
      TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
      List<string> lines = new List<string>((IEnumerable<string>) textAsset.text.Split(new string[3]
      {
        "\r\n",
        "\r",
        "\n"
      }, StringSplitOptions.None));
      foreach (ApplyChangesToSourceCodeTool.SourceCodeChange change in parameters.Changes)
      {
        string[] anchors = change.Anchor.Split(new string[3]
        {
          "\r\n",
          "\r",
          "\n"
        }, StringSplitOptions.None);
        int index = lines.FindIndex((Predicate<string>) (line => line.Trim() == anchors[0].Trim()));
        if (index == -1)
          this.InvokeExecutionFinished(toolCallId, "Error: Anchor line not found in '" + parameters.ClassName + "'. Anchor: '" + change.Anchor + "'");
        string[] collection = change.Content?.Split('\n', StringSplitOptions.None) ?? new string[0];
        switch (change.Action.ToLower())
        {
          case "insertafter":
            lines.InsertRange(index + 1, (IEnumerable<string>) collection);
            break;
          case "insertbefore":
            lines.InsertRange(index, (IEnumerable<string>) collection);
            break;
          case "replace":
            lines.RemoveRange(index, anchors.Length);
            lines.InsertRange(index, (IEnumerable<string>) collection);
            break;
          case "remove":
            lines.RemoveRange(index, anchors.Length);
            break;
          default:
            this.InvokeExecutionFinished(toolCallId, "Error: Invalid action '" + change.Action + "'.");
            return;
        }
      }
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in lines)
        stringBuilder.AppendLine(str);
      DiffWindow.ShowWindow(textAsset.text, stringBuilder.ToString(), (Action<bool, string>) ((result, message) =>
      {
        this.InvokeExecutionFinished(toolCallId, message);
        if (!result)
          return;
        File.WriteAllLines(filePath, (IEnumerable<string>) lines);
        AssetDatabase.Refresh();
      }));
    }

    public class ApplyChangesParameters : ToolParameters
    {
      [JsonProperty("class")]
      [Description("Class name for the target file containing class")]
      public string ClassName;
      [JsonProperty("changes")]
      [Description("List of changes to apply to the source code")]
      public List<ApplyChangesToSourceCodeTool.SourceCodeChange> Changes;
    }

    public class SourceCodeChange
    {
      [JsonProperty("action")]
      [Description("Action to perform: 'insertAfter', 'insertBefore', 'replace', or 'remove'")]
      public string Action;
      [JsonProperty("anchor")]
      [Description("An exact, unique line of source code to anchor the change. The match MUST include all leading whitespace and indentation. It MUST take into account rules of C#.For 'insertAfter', anchor to the LAST line of the preceding code block. For 'insertBefore', anchor to the FIRST line of the succeeding code block. For 'replace' or 'remove', anchor to the EXACT line(s) to be modified. AVOID using comments or blank lines as anchors.")]
      public string Anchor;
      [JsonProperty("content")]
      [Description("Content for the change (for insert and replace actions). It must keep formating and indentations. For multi-line insertions, combine into a single string with '\\n'.")]
      public string Content;
    }
  }
}
