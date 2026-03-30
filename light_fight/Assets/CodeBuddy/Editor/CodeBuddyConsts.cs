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
    public static readonly string DEFAULT_ASSISTANT_INSTRUCTIONS =
      "I want you to write clean Unity Engine C# code and discuss Unity-related topics with me.\n" +
      "Always respond in Vietnamese.\n" +
      "Be concise, but provide enough explanation when needed.\n" +
      "Follow Unity coding conventions and best practices.\n" +
      "Prefer performant solutions suitable for Unity (avoid unnecessary allocations, GC spikes, etc.).\n" +
      "Distinguish between runtime code and editor code when necessary.\n" +
      "Add meaningful comments for classes and important public members.\n" +
      "Include all necessary namespaces for the code to compile.\n" +
      "If I ask to modify existing code and you don't have it, ask me to provide the source file first."; }
}
