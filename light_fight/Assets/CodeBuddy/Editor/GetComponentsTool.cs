// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GetComponentsTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public class GetComponentsTool : ToolBase<GetComponentsTool.GetComponentsToolParameters>
  {
    public GetComponentsTool()
      : base("GetComponents", "Retrieves a list of components and their exposed properties and values based on an instance ID.")
    {
    }

    public override void Execute(
      string toolCallId,
      GetComponentsTool.GetComponentsToolParameters parameters)
    {
      int instanceId = parameters.InstanceId;
      GameObject gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        this.InvokeExecutionFinished(toolCallId, string.Format("No GameObject found with Instance ID: {0}.", (object) instanceId));
      }
      else
      {
        try
        {
          List<GetComponentsTool.ComponentInfo> componentInfoList = new List<GetComponentsTool.ComponentInfo>();
          foreach (UnityEngine.Component component in gameObject.GetComponents<UnityEngine.Component>())
          {
            if (!((UnityEngine.Object) component == (UnityEngine.Object) null))
            {
              GetComponentsTool.ComponentInfo componentInfo = new GetComponentsTool.ComponentInfo()
              {
                name = component.GetType().AssemblyQualifiedName,
                properties = new List<GetComponentsTool.ComponentProperty>()
              };
              foreach (PropertyInfo property in component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
              {
                if (property.CanRead && property.CanWrite)
                {
                  try
                  {
                    object obj = property.GetValue((object) component);
                    componentInfo.properties.Add(new GetComponentsTool.ComponentProperty()
                    {
                      property = property.Name,
                      value = obj != null ? obj.ToString() : "null",
                      propertyType = property.PropertyType.FullName
                    });
                  }
                  catch
                  {
                  }
                }
              }
              foreach (FieldInfo field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
              {
                try
                {
                  object obj = field.GetValue((object) component);
                  componentInfo.properties.Add(new GetComponentsTool.ComponentProperty()
                  {
                    property = field.Name,
                    value = obj != null ? obj.ToString() : "null",
                    propertyType = field.FieldType.FullName
                  });
                }
                catch
                {
                }
              }
              foreach (FieldInfo field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
              {
                if (field.IsDefined(typeof (SerializeField), false))
                {
                  try
                  {
                    object obj = field.GetValue((object) component);
                    componentInfo.properties.Add(new GetComponentsTool.ComponentProperty()
                    {
                      property = field.Name,
                      value = obj != null ? obj.ToString() : "null",
                      propertyType = field.FieldType.FullName
                    });
                  }
                  catch
                  {
                  }
                }
              }
              componentInfoList.Add(componentInfo);
            }
          }
          string result = JsonConvert.SerializeObject((object) componentInfoList, Formatting.Indented);
          this.InvokeExecutionFinished(toolCallId, result);
        }
        catch (Exception ex)
        {
          this.InvokeExecutionFinished(toolCallId, "[FATAL ERROR] Exception during execution : '" + ex.Message + "'");
        }
      }
    }

    public class GetComponentsToolParameters : ToolParameters
    {
      [JsonProperty("instanceId", Required = Required.Always)]
      [Description("The instance ID of the GameObject.")]
      public int InstanceId;
    }

    private class ComponentInfo
    {
      public string name;
      public List<GetComponentsTool.ComponentProperty> properties;
    }

    private class ComponentProperty
    {
      public string property;
      public string value;
      public string propertyType;
    }
  }
}
