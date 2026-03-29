// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.ClaudeMessageConverter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable enable
namespace CodeBuddy.Services
{
  internal class ClaudeMessageConverter : IMessageConverter
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
      if (message.ToolCalls != null && message.ToolCalls.Length != 0)
      {
        if (message.Role == "tool")
        {
          ClaudeMessageConverter.ClaudeMessage completionMessage = new ClaudeMessageConverter.ClaudeMessage();
          completionMessage.role = "user";
          completionMessage.content = new object[1]
          {
            (object) new
            {
              type = "tool_result",
              tool_use_id = message.ToolCalls[0].id,
              content = message.FullContent
            }
          };
          return (CompletionMessageBase) completionMessage;
        }
        ClaudeMessageConverter.ClaudeMessage completionMessage1 = new ClaudeMessageConverter.ClaudeMessage();
        completionMessage1.role = message.Role;
        completionMessage1.content = new object[1]
        {
          (object) new
          {
            type = "tool_use",
            id = message.ToolCalls[0].id,
            name = message.ToolCalls[0].function.name,
            input = JsonConvert.DeserializeObject(message.ToolCalls[0].function.arguments)
          }
        };
        return (CompletionMessageBase) completionMessage1;
      }
      if (message.TexturesGuids != null && message.TexturesGuids.Count > 0)
      {
        List<object> objectList = new List<object>();
        objectList.Add((object) new
        {
          type = "text",
          text = message.FullContent
        });
        foreach (string str in message.ImagesContent)
          objectList.Add((object) new
          {
            type = "image",
            source = new
            {
              type = "base64",
              media_type = "image/png",
              data = str
            }
          });
        ClaudeMessageConverter.ClaudeMessage completionMessage = new ClaudeMessageConverter.ClaudeMessage();
        completionMessage.role = message.Role;
        completionMessage.content = objectList.ToArray();
        return (CompletionMessageBase) completionMessage;
      }
      CompletionMessageText completionMessage2 = new CompletionMessageText();
      completionMessage2.role = message.Role;
      completionMessage2.content = message.FullContent;
      return (CompletionMessageBase) completionMessage2;
    }

    private class ClaudeMessage : CompletionMessageBase
    {
      public object[] content;
    }
  }
}
