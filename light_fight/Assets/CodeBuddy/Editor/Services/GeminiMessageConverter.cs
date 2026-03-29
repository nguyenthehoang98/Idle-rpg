// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.GeminiMessageConverter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.Collections.Generic;

#nullable enable
namespace CodeBuddy.Services
{
  internal class GeminiMessageConverter : IMessageConverter
  {
    public CompletionMessageBase[] ConvertMessages(List<ServiceMessage> messages)
    {
      List<CompletionMessageBase> completionMessageBaseList = new List<CompletionMessageBase>();
      foreach (ServiceMessage message in messages)
        completionMessageBaseList.Add(this.ConvertToCompletionMessage(message));
      return completionMessageBaseList.ToArray();
    }

    private CompletionMessageBase ConvertToCompletionMessage(ServiceMessage message)
    {
      if (message.Role == "tool")
        return (CompletionMessageBase) new GeminiMessageConverter.GeminiFunctionResponseMessage(message.ToolCalls[0].function.name, message.Content);
      if (message.ToolCalls != null && message.ToolCalls.Length != 0)
        return (CompletionMessageBase) new GeminiMessageConverter.GeminiFunctionCallMessage(message.ToolCalls[0].function.name);
      GeminiMessageConverter.GeminiMessage completionMessage = new GeminiMessageConverter.GeminiMessage(message.FullContent, message.ImagesContent);
      completionMessage.role = message.Role == "assistant" ? "model" : "user";
      return (CompletionMessageBase) completionMessage;
    }

    private class GeminiMessage : CompletionMessageBase
    {
      public object[] parts;

      public GeminiMessage(string content, string[] imageData)
      {
        List<object> objectList = new List<object>();
        if (!string.IsNullOrEmpty(content))
          objectList.Add((object) new{ text = content });
        if (imageData != null && imageData.Length != 0)
        {
          foreach (string str in imageData)
            objectList.Add((object) new
            {
              inline_data = new
              {
                mime_type = "image/png",
                data = str
              }
            });
        }
        this.parts = objectList.ToArray();
      }
    }

    private class GeminiFunctionCallMessage : CompletionMessageBase
    {
      public object[] parts;

      public GeminiFunctionCallMessage(string name)
      {
        this.role = "model";
        this.parts = new object[1]
        {
          (object) new{ functionCall = new{ name = name } }
        };
      }
    }

    private class GeminiFunctionResponseMessage : CompletionMessageBase
    {
      public object[] parts;

      public GeminiFunctionResponseMessage(string name, string response)
      {
        this.role = "function";
        this.parts = new object[1]
        {
          (object) new
          {
            functionResponse = new
            {
              name = name,
              response = new{ result = response }
            }
          }
        };
      }
    }
  }
}
