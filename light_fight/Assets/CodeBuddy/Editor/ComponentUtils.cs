// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ComponentUtils
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public static class ComponentUtils
  {
    public static List<string> AddAndAssignComponent(
      GameObject targetObject,
      ComponentToolParameter componentEntry)
    {
      List<string> stringList = new List<string>();
      try
      {
        System.Type type = TypeFinder.FindType(componentEntry.Name, componentEntry.AssemblyName);
        if (type == (System.Type) null || !typeof (Component).IsAssignableFrom(type))
        {
          stringList.Add("[Component: " + componentEntry.Name + "] ERROR: Type '" + componentEntry.Name + "' not found or not a valid Component.");
          return stringList;
        }
        Component component = targetObject.GetComponent(type);
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
          component = targetObject.AddComponent(type);
        stringList.AddRange((IEnumerable<string>) ComponentUtils.AssignComponentValues(component, componentEntry.Values, componentEntry.Name));
        stringList.AddRange((IEnumerable<string>) ComponentUtils.AssignComponentReferences(component, componentEntry.References, componentEntry.Name));
      }
      catch (Exception ex)
      {
        stringList.Add("[Component: " + componentEntry.Name + "] ERROR: Exception during assignmement : '" + ex.Message + "'");
      }
      return stringList;
    }

    private static List<string> AssignComponentReferences(
      Component component,
      ComponentReferenceValueParameter[] references,
      string componentName)
    {
      List<string> stringList = new List<string>();
      if (references == null)
        return stringList;
      System.Type type = component.GetType();
      foreach (ComponentReferenceValueParameter reference in references)
      {
        string property1 = reference.Property;
        string str = reference.InstanceId.ToString();
        PropertyInfo property2 = type.GetProperty(property1);
        if (property2 != (PropertyInfo) null)
        {
          System.Type propertyType = property2.PropertyType;
          GameObject gameObject = EditorUtility.InstanceIDToObject(reference.InstanceId) as GameObject;
          if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
            stringList.Add(string.Format("[Component: {0}] ERROR: Reference property '{1}' could not be assigned to '{2}': No GameObject found with Instance ID: {3}.", (object) componentName, (object) property1, (object) str, (object) reference.InstanceId));
          else if (propertyType == typeof (GameObject))
          {
            property2.SetValue((object) component, (object) gameObject);
            stringList.Add("[Component: " + componentName + "] SUCCESS: Reference property '" + property1 + "' assigned to '" + str + "'.");
          }
          else
          {
            object component1 = (object) gameObject.GetComponent(propertyType);
            if (component1 == null)
            {
              stringList.Add(string.Format("[Component: {0}] ERROR: Reference property '{1}' could not be assigned to '{2}': No {3} found in {4}.", (object) componentName, (object) property1, (object) str, (object) propertyType, (object) gameObject));
            }
            else
            {
              property2.SetValue((object) component, component1);
              stringList.Add("[Component: " + componentName + "] SUCCESS: Reference property '" + property1 + "' assigned to '" + str + "'.");
            }
          }
        }
        else
        {
          FieldInfo field = type.GetField(property1);
          if (field != (FieldInfo) null)
          {
            System.Type fieldType = field.FieldType;
            GameObject gameObject = EditorUtility.InstanceIDToObject(reference.InstanceId) as GameObject;
            if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
              stringList.Add(string.Format("[Component: {0}] ERROR: Reference field '{1}' could not be assigned to '{2}': No GameObject found with Instance ID: {3}.", (object) componentName, (object) property1, (object) str, (object) reference.InstanceId));
            else if (fieldType == typeof (GameObject))
            {
              field.SetValue((object) component, (object) gameObject);
              stringList.Add("[Component: " + componentName + "] SUCCESS: Reference field '" + property1 + "' assigned to '" + str + "'.");
            }
            else
            {
              object component2 = (object) gameObject.GetComponent(fieldType);
              if (component2 == null)
              {
                stringList.Add(string.Format("[Component: {0}] ERROR: Reference field '{1}' could not be assigned to '{2}': No {3} found in {4}.", (object) componentName, (object) property1, (object) str, (object) fieldType, (object) gameObject));
              }
              else
              {
                field.SetValue((object) component, component2);
                stringList.Add("[Component: " + componentName + "] SUCCESS: Reference field '" + property1 + "' assigned to '" + str + "'.");
              }
            }
          }
          else
            stringList.Add("[Component: " + componentName + "] ERROR: Reference '" + property1 + "' could not be assigned to '" + str + "': Property or field not found.");
        }
      }
      return stringList;
    }

    private static List<string> AssignComponentValues(
      Component component,
      ComponentValueParameter[] componentValues,
      string componentName)
    {
      List<string> stringList = new List<string>();
      if (componentValues == null)
        return stringList;
      System.Type type1 = component.GetType();
      foreach (ComponentValueParameter componentValue in componentValues)
      {
        string property1 = componentValue.Property;
        string str = componentValue.Value;
        try
        {
          if (property1.Contains('.'))
          {
            string[] strArray = property1.Split('.', StringSplitOptions.None);
            string name1 = strArray[0];
            PropertyInfo property2 = type1.GetProperty(name1);
            System.Type propertyType = property2.PropertyType;
            for (int index = 1; index < strArray.Length; ++index)
            {
              string name2 = strArray[index];
              property2 = propertyType.GetProperty(name2);
              propertyType = property2.PropertyType;
            }
            object type2 = ComponentUtils.ConvertToType(componentValue, propertyType);
            property2.SetValue((object) component, type2);
            stringList.Add("[Component: " + componentName + "] SUCCESS: Nested property '" + property1 + "' assigned to '" + str + "'.");
          }
          else
          {
            PropertyInfo property3 = type1.GetProperty(property1);
            if (property3 != (PropertyInfo) null)
            {
              System.Type propertyType = property3.PropertyType;
              object type3 = ComponentUtils.ConvertToType(componentValue, propertyType);
              property3.SetValue((object) component, type3);
              stringList.Add("[Component: " + componentName + "] SUCCESS: Property '" + property1 + "' assigned to '" + str + "'.");
            }
            else
            {
              FieldInfo field = type1.GetField(property1);
              if (field != (FieldInfo) null)
              {
                System.Type fieldType = field.FieldType;
                object type4 = ComponentUtils.ConvertToType(componentValue, fieldType);
                field.SetValue((object) component, type4);
                stringList.Add("[Component: " + componentName + "] SUCCESS: Field '" + property1 + "' assigned to '" + str + "'.");
              }
              else
                stringList.Add("[Component: " + componentName + "] ERROR: Property/Field '" + property1 + "' could not be assigned to '" + str + "': Not found.");
            }
          }
        }
        catch (Exception ex)
        {
          stringList.Add("[Component: " + componentName + "] ERROR: Property/Field '" + property1 + "' could not be assigned to '" + str + "': " + ex.Message);
        }
      }
      return stringList;
    }

    private static object ConvertToType(ComponentValueParameter item, System.Type currentType)
    {
      if (AssetDatabase.GetMainAssetTypeAtPath(item.Value) != (System.Type) null)
      {
        UnityEngine.Object type = AssetDatabase.LoadAssetAtPath(item.Value, currentType);
        if (type != (UnityEngine.Object) null)
          return (object) type;
      }
      return UnityTypeParser.HasParser(currentType) ? UnityTypeParser.Parse(currentType, item.Value) : Convert.ChangeType((object) item.Value, currentType);
    }
  }
}
