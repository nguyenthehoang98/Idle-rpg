// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CreateGameObjectTool
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
  public class CreateGameObjectTool : ToolBase<CreateGameObjectTool.CreateGameObjectParameters>
  {
    public CreateGameObjectTool()
      : base("CreateGameObject", "Creates a GameObject with a specified name, optional parent by InstanceId, and optional list of components with their properties.")
    {
    }

    public override void Execute(
      string toolCallId,
      CreateGameObjectTool.CreateGameObjectParameters parameters)
    {
      string name = parameters.Name;
      Vector3 position = parameters.Position;
      Vector3 rotation = parameters.Rotation;
      Vector3 scale = parameters.Scale;
      int parentInstanceId = parameters.ParentInstanceId;
      GameObject gameObject = (GameObject) null;
      if (parentInstanceId != 0)
      {
        gameObject = EditorUtility.InstanceIDToObject(parentInstanceId) as GameObject;
        if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
        {
          this.InvokeExecutionFinished(toolCallId, string.Format("[FATAL ERROR] Parent GameObject with ID {0} not found.", (object) parentInstanceId));
          return;
        }
      }
      GameObject targetObject = new GameObject(name);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        targetObject.transform.SetParent(gameObject.transform);
      List<string> values = new List<string>();
      if (parameters.Components != null)
      {
        foreach (ComponentToolParameter component in parameters.Components)
        {
          List<string> collection = ComponentUtils.AddAndAssignComponent(targetObject, component);
          values.AddRange((IEnumerable<string>) collection);
        }
      }
      try
      {
        targetObject.transform.position = position;
        targetObject.transform.rotation = Quaternion.Euler(rotation);
        targetObject.transform.localScale = scale;
      }
      catch (Exception ex)
      {
        values.Add("[ERROR] Failed to set transform properties: Exception '" + ex.Message + "'");
      }
      values.Add(string.Format("GameObject '{0}' created with Instance ID: {1}", (object) name, (object) targetObject.GetInstanceID()));
      string result = string.Join("\n", (IEnumerable<string>) values);
      this.InvokeExecutionFinished(toolCallId, result);
    }

    public class CreateGameObjectParameters : ToolParameters
    {
      [JsonProperty("name", Required = Required.Always)]
      [Description("The name of the GameObject to create.")]
      public string Name;
      [JsonProperty("position", Required = Required.Always)]
      [JsonConverter(typeof (UnityTypeConverter))]
      [Description("X,Y,Z position of the object")]
      public Vector3 Position;
      [JsonProperty("scale", Required = Required.Always)]
      [JsonConverter(typeof (UnityTypeConverter))]
      [Description("X,Y,Z scale of the object")]
      public Vector3 Scale;
      [JsonProperty("rotation", Required = Required.Always)]
      [JsonConverter(typeof (UnityTypeConverter))]
      [Description("X,Y,Z roation of the object")]
      public Vector3 Rotation;
      [JsonProperty("parentInstanceId")]
      [Description("The optional parent GameObject's InstanceId.")]
      public int ParentInstanceId;
      [JsonProperty("components")]
      [Description("Optional list of component types in assembly qualified names to add to the GameObject with corresponding values to assign.")]
      public ComponentToolParameter[] Components;
    }
  }
}
