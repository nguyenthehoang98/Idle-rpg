// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GetSourceCodeTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  public class GetSourceCodeTool : ToolBase<GetSourceCodeTool.GetSourceCodeParameters>
  {
    public GetSourceCodeTool()
      : base("get_sources", "DANGER: Do not use for reading class data. Use this tool ONLY when the user's request is to MODIFY or REFACTOR the internal logic of an existing C# class. For all other purposes, such as creating new GameObjects or assigning variables, use 'GetPublicMembers' instead.")
    {
    }

    public override void Execute(
      string toolCallId,
      GetSourceCodeTool.GetSourceCodeParameters parameters)
    {
      string filePathForClass = ScriptableSingleton<ProjectInfoGenerator>.instance.GetFilePathForClass(parameters.ClassName);
      if (filePathForClass == null)
      {
        this.InvokeExecutionFinished(toolCallId, parameters.ClassName + " sources not found");
      }
      else
      {
        try
        {
          string result = "-----BEGINNING OF THE FILE-----\n" + AssetDatabase.LoadAssetAtPath<TextAsset>(filePathForClass).text + "\n-----END OF THE FILE-----";
          this.InvokeExecutionFinished(toolCallId, result);
        }
        catch (Exception ex)
        {
          this.InvokeExecutionFinished(toolCallId, "[FATAL ERROR] Exception loading file: '" + ex.Message + "'");
        }
      }
    }

    public class GetSourceCodeParameters : ToolParameters
    {
      [JsonProperty("class")]
      [Description("Class name for the target class")]
      public string ClassName;
    }
  }
}
