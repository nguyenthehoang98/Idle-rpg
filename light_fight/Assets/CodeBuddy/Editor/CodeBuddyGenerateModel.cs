// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeBuddyGenerateModel
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  internal class CodeBuddyGenerateModel : ScriptableSingleton<CodeBuddyGenerateModel>
  {
    private static readonly string TOOL_CALL = "delayedTool_toolCall";
    private static readonly string TOOL_RESULT = "delayedTool_result";
    private FileService fileService;
    private UrlDetectorFetcher urlDetectorFetcher = new UrlDetectorFetcher();
    public ObservableCollection<ServiceMessage> Messages = new ObservableCollection<ServiceMessage>();
    public ObservableCollection<ServiceChat> Chats = new ObservableCollection<ServiceChat>();
    public ObservableCollection<UnityEngine.Object> Attachments = new ObservableCollection<UnityEngine.Object>();
    [SerializeField]
    public string RequestMessage = "";
    public string ChatName = "New chat";
    [SerializeField]
    private string currentChatId = "";
    private ToolCall toolCall = (ToolCall) null;

    private IAiService currentService => ScriptableSingleton<Settings>.instance.Service;

    public ServiceStatus Status => this.currentService.Status;

    public string StatusMessage => this.currentService.StatusMessage;

    private void OnEnable()
    {
      this.fileService = new FileService();
      this.SyncChatList();
      if (string.IsNullOrEmpty(this.currentChatId))
      {
        this.NewChat();
      }
      else
      {
        ServiceChat chat = this.Chats.FirstOrDefault<ServiceChat>((Func<ServiceChat, bool>) (c => c.Id == this.currentChatId));
        if (chat != null)
        {
          string requestMessage = this.RequestMessage;
          this.SelectChat(chat);
          this.RequestMessage = requestMessage;
        }
        else
          this.NewChat();
      }
      CodeBuddyMessageBus.Instance.Subscribe<MAiServiceChanged>(new Action<MAiServiceChanged>(this.ProcessServiceChange));
      CodeBuddyMessageBus.Instance.Subscribe<MToolCall>(new Action<MToolCall>(this.ProcessToolCall));
      ToolManager.Instance.ToolExecutionFinished += new Action<ToolCall, string, bool>(this.ToolExecutionFinished);
    }

    private void OnDisable()
    {
      CodeBuddyMessageBus.Instance.Unsubscribe<MAiServiceChanged>(new Action<MAiServiceChanged>(this.ProcessServiceChange));
      CodeBuddyMessageBus.Instance.Unsubscribe<MToolCall>(new Action<MToolCall>(this.ProcessToolCall));
      ToolManager.Instance.ToolExecutionFinished -= new Action<ToolCall, string, bool>(this.ToolExecutionFinished);
    }

    private void ProcessToolCall(MToolCall call) => this.toolCall = call.ToolCall;

    private void ExecuteToolCalls()
    {
      if (this.toolCall == null)
        return;
      if (string.IsNullOrEmpty(this.toolCall.function.name))
      {
        this.toolCall = (ToolCall) null;
      }
      else
      {
        ServiceMessage serviceMessage = new ServiceMessage()
        {
          Role = "assistant",
          ToolCalls = new ToolCall[1]{ this.toolCall }
        };
        HistoryService.Instance.CurrentChat.Messages.Add(serviceMessage);
        this.Messages.Add(serviceMessage);
        ToolCall toolCall = this.toolCall;
        this.toolCall = (ToolCall) null;
        ToolManager.Instance.ExecuteTool(toolCall);
      }
    }

    private void ToolExecutionFinished(
      ToolCall toolCall,
      string toolResult,
      bool invokeAfterReload)
    {
      if (invokeAfterReload)
      {
        EditorPrefs.SetString(CodeBuddyGenerateModel.TOOL_CALL, JsonConvert.SerializeObject((object) toolCall));
        EditorPrefs.SetString(CodeBuddyGenerateModel.TOOL_RESULT, toolResult);
        ScriptableSingleton<DomainReloadWatcher>.instance.TrackCompilation(new Action<bool>(this.OnCompilationComple));
      }
      else
      {
        ServiceMessage toolResultMessage = new ServiceMessage()
        {
          Role = "tool",
          Content = toolResult,
          ToolCalls = new ToolCall[1]{ toolCall },
          Skip = true
        };
        HistoryService.Instance.CurrentChat.Messages.Add(toolResultMessage);
        if (toolCall.function.name == new TakeScreenshotTool().name)
          HistoryService.Instance.CurrentChat.Messages.Add(new ServiceMessage()
          {
            Role = "user",
            Skip = true,
            ImagesContent = new string[1]
            {
              TakeScreenshotTool.LatestScreenshot
            }
          });
        ServiceMessage responseMessage = new ServiceMessage()
        {
          Role = "assistant",
          Content = ""
        };
        this.Messages.Add(responseMessage);
        CodeBuddyMessageBus.Instance.Publish<MStartStreaming>(new MStartStreaming());
        Task.Run((Action) (() => this.SendMessageToAI(toolResultMessage, responseMessage))).ContinueWith(new Action<Task>(this.ProcessRequestIsFinished), TaskScheduler.FromCurrentSynchronizationContext());
      }
    }

    private void OnCompilationComple(bool hasErrors)
    {
      if (!hasErrors)
        return;
      CodeBuddyGenerateModel.OnRefresh();
    }

    private void ProcessServiceChange(MAiServiceChanged message)
    {
      this.SyncChatList();
      this.NewChat();
    }

    public void StopStreaming()
    {
      this.currentService.CancelRequest();
      CodeBuddyMessageBus.Instance.Publish<MStopStreaming>(new MStopStreaming());
    }

    internal async void SendMessage()
    {
      List<string> urls;
      Dictionary<string, string> urlContent;
      Task sendTask;
      if (string.IsNullOrEmpty(this.RequestMessage) && (this.Attachments == null || this.Attachments.Count == 0))
      {
        Debug.LogWarning((object) "Code Buddy: Request cannot be empty");
        urls = (List<string>) null;
        urlContent = (Dictionary<string, string>) null;
        sendTask = (Task) null;
      }
      else
      {
        urls = this.urlDetectorFetcher.DetectUrls(this.RequestMessage);
        urlContent = new Dictionary<string, string>();
        foreach (string url in urls)
        {
          string content = await this.urlDetectorFetcher.FetchUrlContentAsync(url);
          if (!string.IsNullOrEmpty(content))
            urlContent[url] = content;
          content = (string) null;
        }
        ServiceMessage newMessage = new ServiceMessage()
        {
          Role = "user",
          Content = this.RequestMessage,
          AttachmentsGuids = this.Attachments.Where<UnityEngine.Object>((Func<UnityEngine.Object, bool>) (a => a is TextAsset)).Select<UnityEngine.Object, string>((Func<UnityEngine.Object, string>) (t => this.GetObjectGuid(t))).ToList<string>(),
          TexturesGuids = this.Attachments.Where<UnityEngine.Object>((Func<UnityEngine.Object, bool>) (a => a is Texture2D || a is Sprite)).Select<UnityEngine.Object, string>((Func<UnityEngine.Object, string>) (t => this.GetObjectGuid(t))).ToList<string>(),
          Urls = urlContent
        };
        this.Messages.Add(newMessage);
        HistoryService.Instance.CurrentChat.Messages.Add(newMessage);
        ServiceMessage responseMessage = new ServiceMessage()
        {
          Role = "assistant",
          Content = ""
        };
        this.Messages.Add(responseMessage);
        CodeBuddyMessageBus.Instance.Publish<MStartStreaming>(new MStartStreaming());
        sendTask = Task.Run((Action) (() => this.SendMessageToAI(newMessage, responseMessage))).ContinueWith(new Action<Task>(this.ProcessRequestIsFinished), TaskScheduler.FromCurrentSynchronizationContext());
        this.RequestMessage = "";
        this.Attachments.Clear();
        this.toolCall = (ToolCall) null;
        urls = (List<string>) null;
        urlContent = (Dictionary<string, string>) null;
        sendTask = (Task) null;
      }
    }

    private async void SendMessageToAI(ServiceMessage message, ServiceMessage responseMessage)
    {
      try
      {
        while (this.currentService.Status == ServiceStatus.Validating)
          await Task.Delay(100);
        Action<string> onDataReceived = (Action<string>) (data =>
        {
          responseMessage.Content += data;
          this.ChatName = HistoryService.Instance.CurrentChat.Name;
        });
        if (string.IsNullOrEmpty(HistoryService.Instance.CurrentChat.Id))
          HistoryService.Instance.AddChat(HistoryService.Instance.CurrentChat);
        this.currentService.SendMessageToChat(HistoryService.Instance.CurrentChat, message, onDataReceived);
        if (!string.IsNullOrEmpty(responseMessage.Content))
          HistoryService.Instance.CurrentChat.Messages.Add(responseMessage);
        else
          this.Messages.Remove(responseMessage);
        onDataReceived = (Action<string>) null;
      }
      catch (Exception ex)
      {
        Debug.LogError((object) ex);
        throw ex;
      }
    }

    private void ProcessRequestIsFinished(Task task)
    {
      ScriptableSingleton<CodeBuddyHistory>.instance.Save();
      CodeBuddyMessageBus.Instance.Publish<MStopStreaming>(new MStopStreaming());
      this.currentChatId = HistoryService.Instance.CurrentChat.Id;
      this.SyncChatList();
      EditorMainThreadDispatcher.Enqueue((Action) (() => this.ExecuteToolCalls()));
    }

    private string GetObjectGuid(UnityEngine.Object obj)
    {
      string guid = string.Empty;
      long localId = 0;
      AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out localId);
      return guid;
    }

    internal void NewChat()
    {
      this.ChatName = "New chat";
      this.Messages.Clear();
      this.Attachments.Clear();
      this.currentChatId = "";
      this.RequestMessage = "";
      this.toolCall = (ToolCall) null;
      HistoryService.Instance.CreateNewChat(this.ChatName, "");
    }

    internal void SyncChatList()
    {
      this.Chats.Clear();
      foreach (ServiceChat serviceChat in (IEnumerable<ServiceChat>) HistoryService.Instance.Chats.OrderByDescending<ServiceChat, DateTime>((Func<ServiceChat, DateTime>) (c => c.Time)))
        this.Chats.Add(serviceChat);
    }

    internal void SyncMessages()
    {
      this.Messages.Clear();
      foreach (ServiceMessage serviceMessage in HistoryService.Instance.CurrentChat.Messages.Where<ServiceMessage>((Func<ServiceMessage, bool>) (m => !m.Skip)))
        this.Messages.Add(serviceMessage);
    }

    internal void SelectChat(ServiceChat chat)
    {
      HistoryService.Instance.SetCurrentChat(chat);
      this.SyncChatList();
      this.ChatName = chat.Name;
      this.currentChatId = chat.Id;
      this.SyncMessages();
    }

    internal void SaveClicked(string codeText, string codeType)
    {
      if (string.IsNullOrEmpty(this.SaveFile(codeText, codeType)))
        return;
      AssetDatabase.Refresh();
    }

    private string SaveFile(string codeText, string codeType)
    {
      string extensionFromCodeType = CodeProcessingUtils.GetExtensionFromCodeType(codeType);
      string className = CodeProcessingUtils.GetClassName(codeText);
      bool forceAsk = ScriptableSingleton<Settings>.instance.AskWhereToSave || extensionFromCodeType != "cs" || string.IsNullOrEmpty(className);
      return this.fileService.CreateScriptFile(codeText, className, extensionFromCodeType, forceAsk);
    }

    internal void SaveAndAddClicked(string codeText, string codeType, GameObject gameObject)
    {
      string str = this.SaveFile(codeText, codeType);
      if (string.IsNullOrEmpty(str))
        return;
      EditorPrefs.SetString(CodeBuddyConsts.PREF_PATH, str);
      EditorPrefs.SetInt(CodeBuddyConsts.PREF_OBJECT_NAME, gameObject.GetInstanceID());
      AssetDatabase.Refresh();
    }

    internal void UpdateClicked(string codeText, string filePath)
    {
      this.fileService.UpdateScriptFile(codeText, filePath);
    }

    internal void Detach(UnityEngine.Object asset) => this.Attachments.Remove(asset);

    internal void Attach(UnityEngine.Object asset)
    {
      if (this.Attachments.Contains(asset))
        return;
      this.Attachments.Add(asset);
    }

    [InitializeOnLoadMethod]
    private static void OnRefresh()
    {
      GameObject gameObject = EditorUtility.InstanceIDToObject(EditorPrefs.GetInt(CodeBuddyConsts.PREF_OBJECT_NAME)) as GameObject;
      string str = EditorPrefs.GetString(CodeBuddyConsts.PREF_PATH);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null && !string.IsNullOrEmpty(str))
      {
        string assetPath = str.Substring(Application.dataPath.Length - 6);
        AssetDatabase.GetMainAssetTypeAtPath(assetPath);
        MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
        Undo.AddComponent(gameObject, monoScript.GetClass());
      }
      EditorPrefs.SetString(CodeBuddyConsts.PREF_PATH, "");
      EditorPrefs.SetString(CodeBuddyConsts.PREF_OBJECT_NAME, "");
      if (!EditorPrefs.HasKey(CodeBuddyGenerateModel.TOOL_CALL) || !EditorPrefs.HasKey(CodeBuddyGenerateModel.TOOL_RESULT))
        return;
      ToolCall toolCall = JsonConvert.DeserializeObject<ToolCall>(EditorPrefs.GetString(CodeBuddyGenerateModel.TOOL_CALL));
      string toolResult = EditorPrefs.GetString(CodeBuddyGenerateModel.TOOL_RESULT);
      EditorPrefs.DeleteKey(CodeBuddyGenerateModel.TOOL_CALL);
      EditorPrefs.DeleteKey(CodeBuddyGenerateModel.TOOL_RESULT);
      EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() => EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() => ScriptableSingleton<CodeBuddyGenerateModel>.instance.ToolExecutionFinished(toolCall, toolResult, false)));
    }
  }
}
