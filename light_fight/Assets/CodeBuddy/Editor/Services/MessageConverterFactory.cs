// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.MessageConverterFactory
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

#nullable enable
namespace CodeBuddy.Services
{
  internal static class MessageConverterFactory
  {
    internal static IMessageConverter GetConverter(IAiService aiService)
    {
      switch (aiService)
      {
        case ClaudeService _:
          return (IMessageConverter) new ClaudeMessageConverter();
        case GeminiService _:
          return (IMessageConverter) new GeminiMessageConverter();
        case OllamaService _:
          return (IMessageConverter) new OllamaMessageConverter();
        default:
          return (IMessageConverter) new OpenAiCompletionsMessageConverter();
      }
    }
  }
}
