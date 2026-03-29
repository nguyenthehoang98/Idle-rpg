// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CreateNewScriptTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  public class CreateNewScriptTool : ToolBase<CreateNewScriptTool.CreateNewScriptParameters>
  {
    public CreateNewScriptTool()
      : base("create_new_script", "Creates a new C# script file.")
    {
    }

    public override void Execute(
      string toolCallId,
      CreateNewScriptTool.CreateNewScriptParameters parameters)
    {
      if (string.IsNullOrWhiteSpace(parameters.ScriptName) || string.IsNullOrWhiteSpace(parameters.ScriptCode))
      {
        this.InvokeExecutionFinished(toolCallId, "Error: Script name and code must not be empty.");
      }
      else
      {
        FileService fileService = new FileService();
        try
        {
          string fileName = parameters.ScriptName.EndsWith(".cs") ? parameters.ScriptName.Substring(parameters.ScriptName.Length - 3, 3) : parameters.ScriptName;
          string str = fileService.SaveScript(parameters.ScriptCode, fileName, "cs", ScriptableSingleton<Settings>.instance.AskWhereToSave);
          if (string.IsNullOrEmpty(str))
          {
            this.InvokeExecutionFinished(toolCallId, "Script creation cancelled by user.");
          }
          else
          {
            this.InvokeExecutionFinished(toolCallId, "Script '" + str + "' saved.", true);
            AssetDatabase.Refresh();
          }
        }
        catch (Exception ex)
        {
          this.InvokeExecutionFinished(toolCallId, "Error creating script: " + ex.Message);
        }
      }
    }

    public class CreateNewScriptParameters : ToolParameters
    {
      [JsonProperty("scriptName")]
      [Description("Name of the new script (without .cs extension)")]
      public string ScriptName;
      [JsonProperty("scriptCode")]
      [Description("The complete source code for the new script")]
      public string ScriptCode;
    }
  }
}
