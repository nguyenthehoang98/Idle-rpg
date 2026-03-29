// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ToolParameterAttribute
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;

#nullable enable
namespace CodeBuddy
{
  [AttributeUsage(AttributeTargets.Class)]
  public class ToolParameterAttribute : Attribute
  {
    public Type ParameterType { get; }

    public ToolParameterAttribute(Type type) => this.ParameterType = type;
  }
}
