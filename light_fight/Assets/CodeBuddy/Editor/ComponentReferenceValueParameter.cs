// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ComponentReferenceValueParameter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.ComponentModel;

#nullable enable
namespace CodeBuddy
{
  public class ComponentReferenceValueParameter
  {
    [JsonProperty("property")]
    [Description("Name of the property in target component")]
    public string Property;
    [JsonProperty("propertyType")]
    [Description("Assembly qualified type of the property")]
    public string PropertyType;
    [JsonProperty("propertyTypeAssembly")]
    [Description("Assembly to lookpup this property type")]
    public string PropertyTypeAssembly;
    [JsonProperty("instanceId")]
    [Description("The instance ID of the GameObject to get reference from")]
    public int InstanceId;
  }
}
