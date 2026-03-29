// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.OpenAiResponsesMessageConverter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.Collections.Generic;

#nullable enable
namespace CodeBuddy.Services
{
  internal class OpenAiResponsesMessageConverter
  {
    public object[] ConvertMessages(List<ServiceMessage> messages)
    {
      List<object> objectList = new List<object>();
      foreach (ServiceMessage message in messages)
        objectList.Add(this.ConvertToCompletionMessage(message));
      return objectList.ToArray();
    }

    private object ConvertToCompletionMessage(ServiceMessage message)
    {
      if (message.ToolCalls != null && message.ToolCalls.Length != 0)
      {
        if (message.Role == "tool")
          return (object) new ResponsesToolResultMessage()
          {
            output = message.FullContent,
            call_id = message.ToolCalls[0].id
          };
        return (object) new ResponsesToolCallMessage()
        {
          name = message.ToolCalls[0].function.name,
          arguments = message.ToolCalls[0].function.arguments,
          call_id = message.ToolCalls[0].id
        };
      }
      if (message.TexturesGuids != null && message.TexturesGuids.Count > 0)
      {
        List<MessagePart> messagePartList1 = new List<MessagePart>();
        List<MessagePart> messagePartList2 = messagePartList1;
        MessageTextPart messageTextPart = new MessageTextPart();
        messageTextPart.type = "input_text";
        messageTextPart.text = message.FullContent;
        messagePartList2.Add((MessagePart) messageTextPart);
        foreach (string str in message.ImagesContent)
        {
          List<MessagePart> messagePartList3 = messagePartList1;
          MessageImagePart messageImagePart = new MessageImagePart();
          messageImagePart.type = "input_image";
          messageImagePart.image_url = (object) ("data:image/png;base64," + str);
          messagePartList3.Add((MessagePart) messageImagePart);
        }
        CompletionMessageImage completionMessage = new CompletionMessageImage();
        completionMessage.role = message.Role;
        completionMessage.content = messagePartList1.ToArray();
        return (object) completionMessage;
      }
      CompletionMessageText completionMessage1 = new CompletionMessageText();
      completionMessage1.role = message.Role;
      completionMessage1.content = message.FullContent;
      return (object) completionMessage1;
    }
  }
}
