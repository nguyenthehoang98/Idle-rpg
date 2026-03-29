// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ComponentToolParameter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.ComponentModel;

#nullable enable
namespace CodeBuddy
{
  public class ComponentToolParameter
  {
    [JsonProperty("name", Required = Required.Always)]
    [Description("Assembly qualified name of the component to add to the game object")]
    public string Name;
    [JsonProperty("assemblyName", Required = Required.Always)]
    [Description("Assembly name to lookup this component.")]
    public string AssemblyName;
    [JsonProperty("values")]
    [Description("List of properties and field with values to assign to the component")]
    public ComponentValueParameter[] Values;
    [JsonProperty("references")]
    [Description("List of references to game objets or components to assign")]
    public ComponentReferenceValueParameter[] References;
  }
}
