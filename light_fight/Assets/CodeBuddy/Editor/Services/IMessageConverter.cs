// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.IMessageConverter
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.Collections.Generic;

#nullable enable
namespace CodeBuddy.Services
{
  internal interface IMessageConverter
  {
    CompletionMessageBase[] ConvertMessages(List<ServiceMessage> messages);
  }
}
