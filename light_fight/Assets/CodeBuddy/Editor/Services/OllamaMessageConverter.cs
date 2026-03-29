// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.OllamaMessageConverter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable enable
namespace CodeBuddy.Services
{
  internal class OllamaMessageConverter : IMessageConverter
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
      {
        CompletionOllamaToolResultMessage completionMessage = new CompletionOllamaToolResultMessage();
        completionMessage.role = message.Role;
        completionMessage.content = message.Content;
        completionMessage.tool_name = message.ToolCalls[0].function.name;
        return (CompletionMessageBase) completionMessage;
      }
      if (message.ToolCalls != null && message.ToolCalls.Length != 0)
      {
        CompletionOllamaToolCallMessage completionMessage = new CompletionOllamaToolCallMessage();
        completionMessage.role = message.Role;
        completionMessage.content = message.Content;
        completionMessage.tool_calls = new object[1]
        {
          (object) new
          {
            function = new
            {
              name = message.ToolCalls[0].function.name,
              arguments = JsonConvert.DeserializeObject(message.ToolCalls[0].function.arguments)
            }
          }
        };
        return (CompletionMessageBase) completionMessage;
      }
      if (message.TexturesGuids != null && message.TexturesGuids.Count > 0)
      {
        List<MessagePart> messagePartList = new List<MessagePart>();
        messagePartList.Add((MessagePart) new MessageTextPart()
        {
          text = message.FullContent
        });
        foreach (string str in message.ImagesContent)
          messagePartList.Add((MessagePart) new MessageImagePart()
          {
            image_url = (object) new
            {
              url = ("data:image/png;base64," + str)
            }
          });
        CompletionMessageImage completionMessage = new CompletionMessageImage();
        completionMessage.role = message.Role;
        completionMessage.content = messagePartList.ToArray();
        return (CompletionMessageBase) completionMessage;
      }
      CompletionMessageText completionMessage1 = new CompletionMessageText();
      completionMessage1.role = message.Role;
      completionMessage1.content = message.FullContent;
      return (CompletionMessageBase) completionMessage1;
    }
  }
}
