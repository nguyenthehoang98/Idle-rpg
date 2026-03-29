// Decompiled with JetBrains decompiler
// Type: CodeBuddy.AssignComponentValuesTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public class AssignComponentValuesTool : 
    ToolBase<AssignComponentValuesTool.AssignComponentValuesParameters>
  {
    public AssignComponentValuesTool()
      : base("AssignComponentValues", "Assigns values to components on a GameObject by its instance ID. Adds the component if not present.")
    {
    }

    public override void Execute(
      string toolCallId,
      AssignComponentValuesTool.AssignComponentValuesParameters parameters)
    {
      GameObject targetObject = EditorUtility.InstanceIDToObject(parameters.InstanceId) as GameObject;
      if ((UnityEngine.Object) targetObject == (UnityEngine.Object) null)
      {
        this.InvokeExecutionFinished(toolCallId, string.Format("[FATAL ERROR] GameObject with ID {0} not found.", (object) parameters.InstanceId));
      }
      else
      {
        List<string> values = new List<string>();
        foreach (ComponentToolParameter component in parameters.Components)
        {
          try
          {
            List<string> collection = ComponentUtils.AddAndAssignComponent(targetObject, component);
            values.AddRange((IEnumerable<string>) collection);
          }
          catch (Exception ex)
          {
            values.Add("[ERROR] Failed to assign component value '" + component.Name + ", " + component.AssemblyName + "' : Exception '" + ex.Message + "'");
          }
        }
        string result = string.Join("\n", (IEnumerable<string>) values);
        this.InvokeExecutionFinished(toolCallId, result);
      }
    }

    public class AssignComponentValuesParameters : ToolParameters
    {
      [JsonProperty("instanceId", Required = Required.Always)]
      [Description("The instance ID of the GameObject.")]
      public int InstanceId;
      [JsonProperty("components", Required = Required.Always)]
      [Description("List of component types in assembly qualified names to add or modify on the GameObject.")]
      public ComponentToolParameter[] Components;
    }
  }
}
