// Decompiled with JetBrains decompiler
// Type: CodeBuddy.IAiService
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;

#nullable enable
namespace CodeBuddy
{
  internal interface IAiService
  {
    bool IsStreaming { get; }

    static IAiService Instance { get; }

    ServiceStatus Status { get; }

    string StatusMessage { get; }

    void SendMessageToChat(ServiceChat chat, ServiceMessage message, Action<string> onDataReceived);

    void CancelRequest();

    void Reset();
  }
}
