// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ToolBase`1
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

#nullable enable
namespace CodeBuddy
{
  public abstract class ToolBase<T> : ToolBase where T : ToolParameters
  {
    public ToolBase(string _name, string _description)
      : base(_name, _description)
    {
    }

    public abstract void Execute(string toolCallId, T parameters);

    public string GetMessageForUser(T parameters) => parameters.UserMessage;
  }
}
