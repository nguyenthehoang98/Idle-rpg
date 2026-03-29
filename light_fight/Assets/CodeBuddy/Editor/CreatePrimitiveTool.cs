// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CreatePrimitiveTool
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
  public class CreatePrimitiveTool : ToolBase<CreatePrimitiveTool.CreatePrimitiveParameters>
  {
    public CreatePrimitiveTool()
      : base("CreatePrimitive", "Creates a primitive GameObject (e.g., Cube, Sphere, Capsule) with a specified name, optional parent by InstanceId, and optional list of components with their properties.")
    {
    }

    public override void Execute(
      string toolCallId,
      CreatePrimitiveTool.CreatePrimitiveParameters parameters)
    {
      string name = parameters.Name;
      PrimitiveType primitiveType = parameters.PrimitiveType;
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
      GameObject primitive = GameObject.CreatePrimitive(primitiveType);
      primitive.name = name;
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        primitive.transform.SetParent(gameObject.transform);
      List<string> values = new List<string>();
      if (parameters.Components != null)
      {
        foreach (ComponentToolParameter component in parameters.Components)
          values.AddRange((IEnumerable<string>) ComponentUtils.AddAndAssignComponent(primitive, component));
      }
      try
      {
        primitive.transform.position = position;
        primitive.transform.rotation = Quaternion.Euler(rotation);
        primitive.transform.localScale = scale;
      }
      catch (Exception ex)
      {
        values.Add("[ERROR] Failed to set transform properties: Exception '" + ex.Message + "'");
      }
      values.Add(string.Format("[SUCCESS] Primitive GameObject '{0}' (ID: {1}) of type '{2}' created.", (object) name, (object) primitive.GetInstanceID(), (object) primitiveType));
      string result = string.Join("\n", (IEnumerable<string>) values);
      this.InvokeExecutionFinished(toolCallId, result);
    }

    public class CreatePrimitiveParameters : ToolParameters
    {
      [JsonProperty("name", Required = Required.Always)]
      [Description("The name of the GameObject to create.")]
      public string Name;
      [JsonProperty("primitiveType", Required = Required.Always)]
      [Description("The type of primitive to create (e.g., Cube, Sphere, Capsule, etc.).")]
      public PrimitiveType PrimitiveType;
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
      [Description("X,Y,Z rotation of the object")]
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
