// Decompiled with JetBrains decompiler
// Type: CodeBuddy.JsonToolSchemaGenerator
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using CodeBuddy.Services;
using System.Collections.Generic;

#nullable enable
namespace CodeBuddy
{
  internal static class JsonToolSchemaGenerator
  {
    private static IToolSchemaGenerator openAiGenerator = (IToolSchemaGenerator) new OpenAiToolSchemaGenerator();
    private static IToolSchemaGenerator completionsGenerator = (IToolSchemaGenerator) new CompletionsToolSchemaGenerator();
    private static IToolSchemaGenerator geminiGenerator = (IToolSchemaGenerator) new GeminiToolSchemaGenerator();
    private static IToolSchemaGenerator claudeGenerator = (IToolSchemaGenerator) new ClaudeToolSchemaGenerator();

    public static object[] GenerateSchema(IEnumerable<ToolBase> tools, IAiService serviceType)
    {
      switch (serviceType)
      {
        case GeminiService _:
          return JsonToolSchemaGenerator.geminiGenerator.GenerateToolsSchema(tools);
        case ClaudeService _:
          return JsonToolSchemaGenerator.claudeGenerator.GenerateToolsSchema(tools);
        case OpenAiResponsesService _:
          return JsonToolSchemaGenerator.openAiGenerator.GenerateToolsSchema(tools);
        default:
          return JsonToolSchemaGenerator.completionsGenerator.GenerateToolsSchema(tools);
      }
    }
  }
}
