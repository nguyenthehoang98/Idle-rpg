// Decompiled with JetBrains decompiler
// Type: CodeBuddy.HistoryService
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using CodeBuddy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  internal class HistoryService
  {
    private static HistoryService instance;

    public static HistoryService Instance
    {
      get
      {
        if (HistoryService.instance == null)
          HistoryService.instance = new HistoryService();
        return HistoryService.instance;
      }
    }

    public List<ServiceChat> Chats { get; private set; }

    public ServiceChat CurrentChat { get; private set; }

    private HistoryService()
    {
      this.Chats = new List<ServiceChat>();
      CodeBuddyMessageBus.Instance.Subscribe<MAiServiceChanged>(new Action<MAiServiceChanged>(this.OnServiceChanged));
      this.LoadCurrentServiceHistory();
    }

    private void OnServiceChanged(MAiServiceChanged changed) => this.LoadCurrentServiceHistory();

    private void LoadCurrentServiceHistory()
    {
      switch (ScriptableSingleton<Settings>.instance.Service)
      {
        case DeepSeekService _:
          this.Chats = ScriptableSingleton<CodeBuddyHistory>.instance.DeepSeekHistory;
          break;
        case OllamaService _:
          this.Chats = ScriptableSingleton<CodeBuddyHistory>.instance.OllamaHistory;
          break;
        case OpenAiCompletionsService _:
        case OpenAiResponsesService _:
          this.Chats = ScriptableSingleton<CodeBuddyHistory>.instance.OpenAiCompletionHistory;
          break;
        case GeminiService _:
          this.Chats = ScriptableSingleton<CodeBuddyHistory>.instance.GeminiHistory;
          break;
        case ClaudeService _:
          this.Chats = ScriptableSingleton<CodeBuddyHistory>.instance.ClaudeHistory;
          break;
      }
      if (this.Chats.Count <= 0)
        return;
      this.CurrentChat = this.Chats.Last<ServiceChat>();
    }

    internal void SetCurrentChat(ServiceChat chat)
    {
      if (!this.Chats.Contains(chat))
        return;
      this.CurrentChat = chat;
    }

    internal void CreateNewChat(string chatName, string projectContext)
    {
      this.CurrentChat = new ServiceChat()
      {
        ProjectContext = projectContext,
        Name = chatName,
        Messages = new List<ServiceMessage>(),
        Time = DateTime.Now
      };
    }

    internal void AddChat(ServiceChat currentChat) => this.Chats.Insert(0, currentChat);
  }
}
