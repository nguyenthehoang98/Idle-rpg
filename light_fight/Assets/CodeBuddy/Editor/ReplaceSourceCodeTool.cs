// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ReplaceSourceCodeTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public class ReplaceSourceCodeTool : ToolBase<ReplaceSourceCodeTool.ReplaceSourceCodeParameters>
  {
    public ReplaceSourceCodeTool()
      : base("replace_source_code", "DANGER: Use this tool ONYL for modifying the internal logic of an existing C# file. Changes MUST be correct before applying.")
    {
    }

    public override void Execute(
      string toolCallId,
      ReplaceSourceCodeTool.ReplaceSourceCodeParameters parameters)
    {
      string filePath = ScriptableSingleton<ProjectInfoGenerator>.instance.GetFilePathForClass(parameters.ClassName);
      if (filePath == null)
      {
        this.InvokeExecutionFinished(toolCallId, parameters.ClassName + " sources not found");
      }
      else
      {
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
        if ((UnityEngine.Object) textAsset == (UnityEngine.Object) null)
        {
          this.InvokeExecutionFinished(toolCallId, "Error: Unable to load the source file for class '" + parameters.ClassName + "'.");
        }
        else
        {
          string text = textAsset.text;
          string newCode = parameters.NewCode;
          if (string.IsNullOrWhiteSpace(newCode))
            this.InvokeExecutionFinished(toolCallId, "Error: New code cannot be empty or null.");
          else if (ScriptableSingleton<Settings>.instance.SkipDiffReview)
          {
            File.WriteAllText(filePath, newCode, Encoding.UTF8);
            this.InvokeExecutionFinished(toolCallId, "Source code replaced successfully.", true);
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceSynchronousImport);
          }
          else
            DiffWindow.ShowWindow(text, newCode, (Action<bool, string>) ((result, message) =>
            {
              if (result)
              {
                File.WriteAllText(filePath, newCode, Encoding.UTF8);
                this.InvokeExecutionFinished(toolCallId, message, true);
                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceSynchronousImport);
              }
              else
                this.InvokeExecutionFinished(toolCallId, message);
            }));
        }
      }
    }

    public class ReplaceSourceCodeParameters : ToolParameters
    {
      [JsonProperty("class")]
      [Description("Class name for the target file containing class")]
      public string ClassName;
      [JsonProperty("newCode")]
      [Description("The complete new source code to replace the existing code")]
      public string NewCode;
    }
  }
}
