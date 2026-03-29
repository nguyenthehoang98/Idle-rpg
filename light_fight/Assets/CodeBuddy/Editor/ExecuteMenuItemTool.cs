// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ExecuteMenuItemTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.ComponentModel;
using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  public class ExecuteMenuItemTool : ToolBase<ExecuteMenuItemTool.ExecuteMenuItemParameters>
  {
    public ExecuteMenuItemTool()
      : base("ExecuteMenuItem", "Executes a specified menu item in the Unity Editor.")
    {
    }

    public override void Execute(
      string toolCallId,
      ExecuteMenuItemTool.ExecuteMenuItemParameters parameters)
    {
      string menuItem = parameters.MenuItem;
      if (EditorApplication.ExecuteMenuItem(menuItem))
        this.InvokeExecutionFinished(toolCallId, "Successfully executed menu item: " + menuItem);
      else
        this.InvokeExecutionFinished(toolCallId, "Failed to execute menu item: " + menuItem);
    }

    public class ExecuteMenuItemParameters : ToolParameters
    {
      [JsonProperty("menuItem", Required = Required.Always)]
      [Description("The menu item to execute (e.g., 'File/Save', 'Edit/Undo').")]
      public string MenuItem;
    }
  }
}
