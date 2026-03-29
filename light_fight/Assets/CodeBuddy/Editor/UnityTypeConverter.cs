// Decompiled with JetBrains decompiler
// Type: CodeBuddy.UnityTypeConverter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public class UnityTypeConverter : JsonConverter
  {
    public override bool CanConvert(System.Type objectType)
    {
      return objectType == typeof (Vector3) || objectType == typeof (Color) || objectType == typeof (Vector2) || objectType == typeof (Vector4) || objectType == typeof (Rect) || objectType == typeof (Bounds) || objectType == typeof (Matrix4x4) || objectType == typeof (Quaternion);
    }

    public override object ReadJson(
      JsonReader reader,
      System.Type objectType,
      object existingValue,
      JsonSerializer serializer)
    {
      string objectString = reader.TokenType == JsonToken.String ? reader.Value.ToString() : throw new JsonSerializationException(string.Format("Expected string for type {0}, but got {1}.", (object) objectType.Name, (object) reader.TokenType));
      return UnityTypeParser.Parse(objectType, objectString);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      if (value != null)
        writer.WriteValue(value.ToString());
      else
        writer.WriteNull();
    }
  }
}
