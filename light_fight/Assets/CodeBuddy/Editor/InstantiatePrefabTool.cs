// Decompiled with JetBrains decompiler
// Type: CodeBuddy.InstantiatePrefabTool
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
  public class InstantiatePrefabTool : ToolBase<InstantiatePrefabTool.InstantiatePrefabParameters>
  {
    public InstantiatePrefabTool()
      : base("InstantiatePrefab", "Instantiates a prefab at a specified position and rotation based on the provided path.")
    {
    }

    public override void Execute(
      string toolCallId,
      InstantiatePrefabTool.InstantiatePrefabParameters parameters)
    {
      string prefabPath = parameters.PrefabPath;
      Vector3 position = parameters.Position;
      Vector3 rotation = parameters.Rotation;
      int parentInstanceId = parameters.ParentInstanceId;
      string objectName = parameters.ObjectName;
      GameObject gameObject1 = (GameObject) null;
      if (parentInstanceId != 0)
      {
        gameObject1 = EditorUtility.InstanceIDToObject(parentInstanceId) as GameObject;
        if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
        {
          this.InvokeExecutionFinished(toolCallId, string.Format("[FATAL ERROR] Parent GameObject with Instance ID {0} not found.", (object) parentInstanceId));
          return;
        }
      }
      GameObject assetComponentOrGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
      if ((UnityEngine.Object) assetComponentOrGameObject == (UnityEngine.Object) null)
      {
        this.InvokeExecutionFinished(toolCallId, "[FATAL ERROR] Prefab not found at path: " + prefabPath);
      }
      else
      {
        GameObject gameObject2 = PrefabUtility.InstantiatePrefab((UnityEngine.Object) assetComponentOrGameObject) as GameObject;
        if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
          gameObject2.transform.SetParent(gameObject1.transform);
        try
        {
          gameObject2.transform.position = position;
          gameObject2.transform.rotation = Quaternion.Euler(rotation);
          if (string.IsNullOrEmpty(objectName))
            gameObject2.name = objectName;
          this.InvokeExecutionFinished(toolCallId, string.Format("Prefab '{0}', ID {1} instantiated successfully at position {2} with rotation {3} with name {4}.", (object) assetComponentOrGameObject.name, (object) gameObject2.GetInstanceID(), (object) position, (object) rotation, (object) gameObject2.name));
        }
        catch (Exception ex)
        {
          this.InvokeExecutionFinished(toolCallId, string.Format("Prefab '{0}', ID {1} instantiated successfully. ", (object) assetComponentOrGameObject.name, (object) gameObject2.GetInstanceID()) + "Failed to assign transform values : '" + ex.Message + "' ");
        }
      }
    }

    public class InstantiatePrefabParameters : ToolParameters
    {
      [JsonProperty("prefabPath", Required = Required.Always)]
      [Description("The path to the prefab to instantiate.")]
      public string PrefabPath;
      [JsonProperty("position", Required = Required.Always)]
      [Description("X,Y,Z position of the object.")]
      [JsonConverter(typeof (UnityTypeConverter))]
      public Vector3 Position;
      [JsonProperty("rotation", Required = Required.Always)]
      [Description("X,Y,Z rotation of the object.")]
      [JsonConverter(typeof (UnityTypeConverter))]
      public Vector3 Rotation;
      [JsonProperty("objectName")]
      [Description("Optional name of the newly created object.")]
      public string ObjectName;
      [JsonProperty("parentInstanceId")]
      [Description("The optional parent GameObject's InstanceId.")]
      public int ParentInstanceId;
    }
  }
}
