// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GeminiToolSchemaGenerator
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

#nullable enable
namespace CodeBuddy
{
  internal class GeminiToolSchemaGenerator : IToolSchemaGenerator
  {
    public object[] GenerateToolsSchema(IEnumerable<ToolBase> tools)
    {
      List<object> objectList = new List<object>();
      foreach (ToolBase tool in tools)
      {
        Dictionary<string, object> schema = this.GenerateSchema(tool.GetType().BaseType.GenericTypeArguments[0]);
        var data = new
        {
          name = tool.name,
          description = tool.description,
          parameters = schema
        };
        objectList.Add((object) data);
      }
      return objectList.ToArray();
    }

    public Dictionary<string, object> GenerateSchema(Type modelType)
    {
      Dictionary<string, object> schema1 = new Dictionary<string, object>()
      {
        ["type"] = (object) "object",
        ["properties"] = (object) new Dictionary<string, object>(),
        ["required"] = (object) new List<string>()
      };
      Dictionary<string, object> dictionary = (Dictionary<string, object>) schema1["properties"];
      List<string> stringList = (List<string>) schema1["required"];
      foreach (FieldInfo field in modelType.GetFields())
      {
        string key = field.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? field.Name;
        Dictionary<string, object> schema2 = new Dictionary<string, object>();
        DescriptionAttribute customAttribute1 = field.GetCustomAttribute<DescriptionAttribute>();
        if (customAttribute1 != null)
          schema2["description"] = (object) customAttribute1.Description;
        stringList.Add(key);
        JsonConverterAttribute customAttribute2 = field.GetCustomAttribute<JsonConverterAttribute>();
        if (customAttribute2 != null && typeof (UnityTypeConverter).IsAssignableFrom(customAttribute2.ConverterType))
          schema2["type"] = (object) "string";
        else
          this.PopulateTypeInfo(field.FieldType, schema2);
        dictionary[key] = (object) schema2;
      }
      if (stringList.Count == 0)
        schema1.Remove("required");
      return schema1;
    }

    private void PopulateTypeInfo(Type type, Dictionary<string, object> schema)
    {
      if (type.IsEnum)
      {
        schema[nameof (type)] = (object) "string";
        schema["enum"] = (object) Enum.GetNames(type);
      }
      else if (typeof (IEnumerable).IsAssignableFrom(type) && type != typeof (string))
      {
        schema[nameof (type)] = (object) "array";
        Type type1 = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
        Dictionary<string, object> schema1 = new Dictionary<string, object>();
        this.PopulateTypeInfo(type1, schema1);
        schema["items"] = (object) schema1;
      }
      else if (type.IsClass && type != typeof (string))
      {
        schema[nameof (type)] = (object) "object";
        Dictionary<string, object> schema2 = this.GenerateSchema(type);
        schema["properties"] = schema2["properties"];
        if (!schema2.ContainsKey("required"))
          return;
        schema["required"] = schema2["required"];
      }
      else
        schema[nameof (type)] = (object) GeminiToolSchemaGenerator.GetJsonType(type);
    }

    private static string GetJsonType(Type type)
    {
      if (UnityTypeParser.HasParser(type) || type == typeof (string))
        return "string";
      if (type == typeof (int) || type == typeof (long) || type == typeof (float) || type == typeof (double) || type == typeof (Decimal))
        return "number";
      return type == typeof (bool) ? "boolean" : "object";
    }
  }
}
