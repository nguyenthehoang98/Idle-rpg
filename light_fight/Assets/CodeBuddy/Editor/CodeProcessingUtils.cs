// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeProcessingUtils
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.Text.RegularExpressions;

#nullable enable
namespace CodeBuddy
{
  internal static class CodeProcessingUtils
  {
    public static string GetClassName(string responseMessage)
    {
      string pattern = "(?<!//.*)(?<!/\\*.*)\\bclass\\s+(\\w+)";
      foreach (Match match in Regex.Matches(responseMessage, pattern))
      {
        if (match.Success)
          return match.Groups[1].Value;
      }
      return "";
    }

    public static string GetExtensionFromCodeType(string codeType)
    {
      switch (codeType)
      {
        case "csharp":
          return "cs";
        case "xml":
          return "uxml";
        case "css":
          return "uss";
        default:
          return "txt";
      }
    }
  }
}
