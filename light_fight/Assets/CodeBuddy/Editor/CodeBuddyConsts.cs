// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeBuddyConsts
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

#nullable enable
namespace CodeBuddy
{
  internal static class CodeBuddyConsts
  {
    public static readonly string PREFS_OPENAI_KEY = "OpenAIKey";
    public static readonly string PREF_ASSISTANT = "CodeBuddyAssistsntId";
    public static readonly string PREF_OBJECT_NAME = "selected_name";
    public static readonly string PREF_PATH = "component_path";
    public static readonly string DEFAULT_ASSISTANT_NAME = "CodeBuddyAssistant";
    public static readonly string DEFAULT_ASSISTANT_INSTRUCTIONS = "I want you to write clean Unity Engine C# code by request and have a conversation with me about my project or unity in general.\r\nBe very concise in your responses.\r\nThe code must follow Unity Codestyle. \r\nIn case of a class or method add a comment section to it. \r\nDo not comment on every line of code. \r\nYou must add comments to the class itself and all public members of the class. \r\nInclude all necessary namespaces for the code to compile.\r\nYou must ask for source code file if I ask you to modify existing code and you dont have it in the conversation.";
  }
}
