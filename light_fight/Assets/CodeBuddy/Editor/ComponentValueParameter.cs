// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ComponentValueParameter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.ComponentModel;

#nullable enable
namespace CodeBuddy
{
  public class ComponentValueParameter
  {
    [JsonProperty("property")]
    [Description("Name of the property or field")]
    public string Property;
    [JsonProperty("propertyType")]
    [Description("Assembly qualified type of the property or field")]
    public string PropertyType;
    [JsonProperty("value")]
    [Description("Value to assign to the property/field. For vector like values (vectors, matrix, colors) components must be splitted with comma, without any additional characters.")]
    public string Value;
  }
}
