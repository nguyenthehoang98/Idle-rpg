// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.OpenAiCompletionsMessageConverter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.Collections.Generic;

#nullable enable
namespace CodeBuddy.Services
{
  internal class OpenAiCompletionsMessageConverter : IMessageConverter
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
          CompletionToolResultMessage completionMessage = new CompletionToolResultMessage();
          completionMessage.role = message.Role;
          completionMessage.content = message.FullContent;
          completionMessage.tool_call_id = message.ToolCalls[0].id;
          return (CompletionMessageBase) completionMessage;
        }
        CompletionToolCallMessage completionMessage1 = new CompletionToolCallMessage();
        completionMessage1.role = message.Role;
        completionMessage1.tool_calls = message.ToolCalls;
        return (CompletionMessageBase) completionMessage1;
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
      CompletionMessageText completionMessage2 = new CompletionMessageText();
      completionMessage2.role = message.Role;
      completionMessage2.content = message.FullContent;
      return (CompletionMessageBase) completionMessage2;
    }
  }
}
