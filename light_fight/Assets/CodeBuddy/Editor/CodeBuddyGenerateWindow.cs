// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeBuddyGenerateWindow
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable
namespace CodeBuddy
{
  internal class CodeBuddyGenerateWindow : EditorWindow
  {
    private bool isPickerOpen = false;
    private bool isStreaming = false;
    private VisualTreeAsset generateWindowAsset;
    [SerializeField]
    private VisualTreeAsset messageTemplate;
    private VisualElement loadingPanel;
    private VisualElement failedPanel;
    private VisualElement historyPanel;
    private ListView messagesListView;
    private ListView chatsListView;
    private Scroller messagesScroller;
    private Button stopButton;
    private Button sendButton;
    private Button attachButton;
    private TextField requestField;
    private ScrollView requestScroll;
    private bool wasScrolledToBottom = true;

    private CodeBuddyGenerateModel model => ScriptableSingleton<CodeBuddyGenerateModel>.instance;

    private void OnEnable()
    {
      CodeBuddyMessageBus.Instance.Subscribe<ServiceStatus>(new Action<ServiceStatus>(this.OnStatusChanged));
      CodeBuddyMessageBus.Instance.Subscribe<MStartStreaming>(new Action<MStartStreaming>(this.StreamingStarted));
      CodeBuddyMessageBus.Instance.Subscribe<MStopStreaming>(new Action<MStopStreaming>(this.StreamingStoped));
      UnityEditor.Selection.selectionChanged += new Action(this.OnSelectionChanged);
    }

    private void OnSelectionChanged() => this.messagesListView?.RefreshItems();

    private void OnDisable()
    {
      if (this.requestField != null)
        this.requestField.UnregisterValueChangedCallback<string>(new EventCallback<ChangeEvent<string>>(this.OnRequestFieldValueChanged));
      if (this.requestScroll != null)
        this.requestScroll.UnregisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>) (evt => this.CheckIfScrolledToBottom()));
      CodeBuddyMessageBus.Instance.Unsubscribe<ServiceStatus>(new Action<ServiceStatus>(this.OnStatusChanged));
      CodeBuddyMessageBus.Instance.Unsubscribe<MStartStreaming>(new Action<MStartStreaming>(this.StreamingStarted));
      CodeBuddyMessageBus.Instance.Unsubscribe<MStopStreaming>(new Action<MStopStreaming>(this.StreamingStoped));
      UnityEditor.Selection.selectionChanged -= new Action(this.OnSelectionChanged);
    }

    private void StreamingStoped(MStopStreaming streaming)
    {
      this.isStreaming = false;
      this.sendButton.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.Flex;
      this.stopButton.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
    }

    private void StreamingStarted(MStartStreaming streaming)
    {
      this.isStreaming = true;
      this.sendButton.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
      this.stopButton.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.Flex;
    }

    private void OnStatusChanged(ServiceStatus status)
    {
      if (this.failedPanel == null || this.loadingPanel == null)
        return;
      switch (status)
      {
        case ServiceStatus.NotInitialized:
        case ServiceStatus.Validating:
          this.failedPanel.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
          this.loadingPanel.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.Flex;
          break;
        case ServiceStatus.Active:
          this.failedPanel.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
          this.loadingPanel.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
          break;
        case ServiceStatus.Failed:
          this.loadingPanel.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
          this.failedPanel.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.Flex;
          this.failedPanel.Q<Label>("m_statusLabel", (string) null).text = this.model.StatusMessage;
          break;
      }
    }

    private void Update()
    {
      if (!this.isStreaming)
        return;
      this.messagesListView.RefreshItem(this.messagesListView.itemsSource.Count - 1);
      if (this.messagesScroller != null)
        this.messagesScroller.value = Mathf.Max(0.0f, this.messagesScroller.highValue);
      this.Repaint();
    }

    private void OnGUI()
    {
      if (!this.isPickerOpen || UnityEngine.Event.current == null || !(UnityEngine.Event.current.commandName == "ObjectSelectorClosed"))
        return;
      UnityEngine.Object objectPickerObject = EditorGUIUtility.GetObjectPickerObject();
      int num;
      if (objectPickerObject != (UnityEngine.Object) null)
      {
        switch (objectPickerObject)
        {
          case TextAsset _:
          case Sprite _:
            num = 1;
            break;
          default:
            num = objectPickerObject is Texture2D ? 1 : 0;
            break;
        }
      }
      else
        num = 0;
      if (num != 0)
      {
        this.model.Attach(objectPickerObject);
        this.isPickerOpen = false;
      }
      this.Repaint();
    }

    private void CreateGUI()
    {
      if ((UnityEngine.Object) this.generateWindowAsset == (UnityEngine.Object) null)
        this.generateWindowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("b511afa433e4af64caecbd764829505a") + "/CodeWindow.uxml");
      if ((UnityEngine.Object) this.messageTemplate == (UnityEngine.Object) null)
        this.messageTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("b511afa433e4af64caecbd764829505a") + "/ChatMessageComponent.uxml");
      this.generateWindowAsset.CloneTree(this.rootVisualElement);
      this.CreateStatusPanels();
      this.historyPanel = this.rootVisualElement.Q<VisualElement>("m_historyPanel", (string) null);
      this.historyPanel.style.display = (StyleEnum<DisplayStyle>) DisplayStyle.None;
      this.sendButton = this.rootVisualElement.Q<Button>("m_sendButton", (string) null);
      this.sendButton.clicked += new Action(this.SendButton_clicked);
      this.stopButton = this.rootVisualElement.Q<Button>("m_stopButton", (string) null);
      this.stopButton.clicked += new Action(this.StopButton_clicked);
      this.rootVisualElement.Q<Button>("m_historyButton", (string) null).clicked += new Action(this.HistoryButton_clicked);
      this.rootVisualElement.Q<Button>("m_newChatButton", (string) null).clicked += new Action(this.NewChatButton_clicked);
      this.CreateMessageElements();
      this.CreateChatListElements();
      this.CreateAttachmentElements();
      this.rootVisualElement.Bind(new SerializedObject((UnityEngine.Object) this.model));
      EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() => EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() => this.messagesScroller.value = Mathf.Max(0.0f, this.messagesScroller.highValue)));
      this.OnStatusChanged(this.model.Status);
      this.requestField = this.rootVisualElement.Q<TextField>("m_requestField", (string) null);
      this.requestField.SetEnabled(true);
      this.rootVisualElement.Q<Label>("m_chatNameLabel", (string) null).SetEnabled(true);
      this.requestField.RegisterCallback<KeyDownEvent>((EventCallback<KeyDownEvent>) (evt =>
      {
        if (!evt.ctrlKey || evt.keyCode != KeyCode.Return)
          return;
        this.SendButton_clicked();
        evt.StopPropagation();
      }));
      this.requestScroll = this.rootVisualElement.Q<ScrollView>("m_requestScroll", (string) null);
      this.requestScroll.RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>) (evt =>
      {
        this.requestScroll.style.maxHeight = (StyleLength) (this.rootVisualElement.resolvedStyle.height * 0.3f);
        this.CheckIfScrolledToBottom();
      }));
      this.requestField.RegisterValueChangedCallback<string>(new EventCallback<ChangeEvent<string>>(this.OnRequestFieldValueChanged));
      this.CheckIfScrolledToBottom();
    }

    private void CheckIfScrolledToBottom()
    {
      if (this.requestScroll == null || this.requestScroll.verticalScroller == null)
        return;
      Scroller verticalScroller = this.requestScroll.verticalScroller;
      float num = 5f;
      this.wasScrolledToBottom = (double) verticalScroller.highValue - (double) verticalScroller.value <= (double) num;
      if ((double) verticalScroller.highValue > 0.0)
        return;
      this.wasScrolledToBottom = true;
    }

    private void OnRequestFieldValueChanged(ChangeEvent<string> evt)
    {
      if (this.wasScrolledToBottom && this.requestScroll != null)
        this.requestScroll.schedule.Execute((Action) (() =>
        {
          if (this.requestScroll == null || this.requestScroll.verticalScroller == null)
            return;
          this.requestScroll.verticalScroller.value = this.requestScroll.verticalScroller.highValue;
          this.wasScrolledToBottom = true;
        })).ExecuteLater(1L);
      else
        this.CheckIfScrolledToBottom();
    }

    private void StopButton_clicked() => this.model.StopStreaming();

    private void CreateStatusPanels()
    {
      this.loadingPanel = this.rootVisualElement.Q<VisualElement>("m_loadingPanel", (string) null);
      this.failedPanel = this.rootVisualElement.Q<VisualElement>("m_failedPanel", (string) null);
      this.rootVisualElement.Q<Button>("m_goToSettingsButton", (string) null).clicked += (Action) (() => SettingsService.OpenProjectSettings("Project/Code Buddy"));
    }

    private void CreateChatListElements()
    {
      this.chatsListView = this.rootVisualElement.Q<ListView>("m_chatsList", (string) null);
      this.chatsListView.makeItem = new Func<VisualElement>(this.MakeChatListItem);
      this.chatsListView.bindItem = (Action<VisualElement, int>) ((e, i) =>
      {
        Label label = e.Q<Label>();
        label.text = label.tooltip = this.model.Chats[i].Name;
      });
      this.chatsListView.onSelectionChange += new Action<IEnumerable<object>>(this.ChatsListView_selectionChanged);
      this.model.Chats.CollectionChanged += (NotifyCollectionChangedEventHandler) ((_1, _2) => this.chatsListView.RefreshItems());
      this.chatsListView.itemsSource = (IList) this.model.Chats;
    }

    private void CreateAttachmentElements()
    {
      ListView attachmentList = this.rootVisualElement.Q<ListView>("m_attachmentList", (string) null);
      attachmentList.style.overflow = (StyleEnum<Overflow>) Overflow.Visible;
      attachmentList.style.width = (StyleLength) Length.Percent(100f);
      attachmentList.style.flexDirection = (StyleEnum<FlexDirection>) FlexDirection.Row;
      attachmentList.style.flexWrap = (StyleEnum<Wrap>) Wrap.Wrap;
      attachmentList.style.flexGrow = (StyleFloat) 1f;
      attachmentList.style.flexShrink = (StyleFloat) 1f;
      attachmentList.style.justifyContent = (StyleEnum<Justify>) Justify.FlexStart;
      attachmentList.makeItem = new Func<VisualElement>(this.MakeAttachmentListItem);
      attachmentList.bindItem = (Action<VisualElement, int>) ((e, i) =>
      {
        e.userData = (object) this.model.Attachments[i];
        e.Q<ObjectField>().value = this.model.Attachments[i];
      });
      this.model.Attachments.CollectionChanged += (NotifyCollectionChangedEventHandler) ((_1, _2) =>
      {
        attachmentList.RefreshItems();
        this.Repaint();
      });
      attachmentList.itemsSource = (IList) this.model.Attachments;
      this.attachButton = this.rootVisualElement.Q<Button>("m_attachButton", (string) null);
      this.attachButton.clicked += (Action) (() =>
      {
        EditorGUIUtility.ShowObjectPicker<UnityEngine.Object>((UnityEngine.Object) null, false, "", 1234);
        this.isPickerOpen = true;
      });
      this.rootVisualElement.RegisterCallback<DragUpdatedEvent>((EventCallback<DragUpdatedEvent>) (evt =>
      {
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        evt.StopPropagation();
      }));
      this.rootVisualElement.RegisterCallback<DragPerformEvent>(new EventCallback<DragPerformEvent>(this.ProcessDragPerformEvent));
    }

    private void ProcessDragPerformEvent(DragPerformEvent evt)
    {
      DragAndDrop.AcceptDrag();
      foreach (UnityEngine.Object objectReference in DragAndDrop.objectReferences)
      {
        int num;
        switch (objectReference)
        {
          case TextAsset _:
          case Texture2D _:
            num = 1;
            break;
          default:
            num = objectReference is Sprite ? 1 : 0;
            break;
        }
        if (num != 0)
          this.model.Attach(objectReference);
        else if (objectReference is DefaultAsset assetObject)
          this.AttachFilesFromFolder(AssetDatabase.GetAssetPath((UnityEngine.Object) assetObject));
      }
      evt.StopPropagation();
    }

    private void AttachFilesFromFolder(string folderPath)
    {
      foreach (string file in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
      {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
        int num;
        if (asset != (UnityEngine.Object) null)
        {
          switch (asset)
          {
            case TextAsset _:
            case Texture2D _:
              num = 1;
              break;
            default:
              num = asset is Sprite ? 1 : 0;
              break;
          }
        }
        else
          num = 0;
        if (num != 0)
          this.model.Attach(asset);
      }
    }

    private void CreateMessageElements()
    {
      this.messagesListView = this.rootVisualElement.Q<ListView>("m_messagesList", (string) null);
      this.messagesScroller = this.messagesListView.Q<Scroller>();
      this.messagesListView.makeItem = (Func<VisualElement>) (() =>
      {
        TemplateContainer e = this.messageTemplate.CloneTree();
        ChatMessageComponent messageComponent = e.Q<ChatMessageComponent>();
        messageComponent.SaveClicked += new Action<string, string>(this.model.SaveClicked);
        messageComponent.SaveAndAddClicked += new Action<string, string, GameObject>(this.model.SaveAndAddClicked);
        messageComponent.UpdateClicked += new Action<string, string>(this.model.UpdateClicked);
        return (VisualElement) e;
      });
      this.messagesListView.destroyItem = (Action<VisualElement>) (item =>
      {
        ChatMessageComponent messageComponent = item.Q<ChatMessageComponent>();
        messageComponent.SaveClicked -= new Action<string, string>(this.model.SaveClicked);
        messageComponent.SaveAndAddClicked -= new Action<string, string, GameObject>(this.model.SaveAndAddClicked);
        messageComponent.UpdateClicked -= new Action<string, string>(this.model.UpdateClicked);
      });
      this.messagesListView.bindItem = new Action<VisualElement, int>(this.BindChatMessageToList);
      this.messagesListView.unbindItem = new Action<VisualElement, int>(this.UnbindChatMessageFromList);
      this.model.Messages.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Messages_CollectionChanged);
      this.messagesListView.itemsSource = (IList) this.model.Messages;
    }

    private VisualElement MakeAttachmentListItem()
    {
      VisualElement container = new VisualElement();
      container.style.flexDirection = (StyleEnum<FlexDirection>) FlexDirection.Row;
      container.style.alignItems = (StyleEnum<Align>) Align.FlexStart;
      ObjectField child1 = new ObjectField();
      child1.style.flexGrow = (StyleFloat) 1f;
      child1.style.flexShrink = (StyleFloat) 1f;
      container.Add((VisualElement) child1);
      Button button = new Button();
      button.text = "✖";
      Button child2 = button;
      child2.style.width = (StyleLength) 20f;
      child2.style.height = (StyleLength) 20f;
      child2.style.unityTextAlign = (StyleEnum<TextAnchor>) TextAnchor.MiddleCenter;
      child2.style.marginLeft = (StyleLength) 5f;
      child2.clicked += (Action) (() => this.DeleteAttachmentButtonClicked(container.userData as UnityEngine.Object));
      container.Add((VisualElement) child2);
      return container;
    }

    private void DeleteAttachmentButtonClicked(UnityEngine.Object asset)
    {
      this.model.Detach(asset);
    }

    private VisualElement MakeChatListItem()
    {
      Label label = new Label();
      label.style.unityTextAlign = (StyleEnum<TextAnchor>) TextAnchor.MiddleLeft;
      return (VisualElement) label;
    }

    private void ChatsListView_selectionChanged(IEnumerable<object> obj)
    {
      this.model.SelectChat(obj.First<object>() as ServiceChat);
      this.HistoryButton_clicked();
    }

    private void BindChatMessageToList(VisualElement e, int i)
    {
      ServiceMessage message = this.model.Messages[i];
      ChatMessageComponent messageComponent = e.Q<ChatMessageComponent>();
      messageComponent.MessageContent = message.Content;
      if (message.ToolCalls != null && ((IEnumerable<ToolCall>) message.ToolCalls).Count<ToolCall>() > 0)
      {
        ToolCall toolCall = message.ToolCalls[0];
        messageComponent.MessageContent = ToolManager.Instance.GetToolMessageForUser(toolCall);
      }
      if (message.AttachmentsGuids != null && message.AttachmentsGuids.Count > 0 || message.TexturesGuids != null && message.TexturesGuids.Count > 0)
      {
        List<UnityEngine.Object> objectList = new List<UnityEngine.Object>();
        foreach (string guid in message.AttachmentsGuids.Concat<string>((IEnumerable<string>) message.TexturesGuids))
        {
          UnityEngine.Object @object = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid));
          objectList.Add(@object);
        }
        messageComponent.Attachments = objectList;
      }
      else
        messageComponent.Attachments = (List<UnityEngine.Object>) null;
      VisualElement visualElement = e.Q<VisualElement>("m_messagePanel", (string) null);
      if (message.Role == "user")
      {
        visualElement.style.marginRight = (StyleLength) 5f;
        visualElement.style.marginLeft = (StyleLength) 30f;
      }
      else
      {
        visualElement.style.marginRight = (StyleLength) 30f;
        visualElement.style.marginLeft = (StyleLength) 5f;
      }
    }

    private void UnbindChatMessageFromList(VisualElement e, int i)
    {
    }

    private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
      {
        this.messagesListView.RefreshItems();
        this.Repaint();
        if (this.messagesScroller == null)
          return;
        this.messagesScroller.value = Mathf.Max(0.0f, this.messagesScroller.highValue);
      });
    }

    internal void NewChatButton_clicked() => this.model.NewChat();

    private void HistoryButton_clicked()
    {
      this.historyPanel.style.display = (StyleEnum<DisplayStyle>) (this.historyPanel.style.display == (StyleEnum<DisplayStyle>) DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex);
    }

    private void SendButton_clicked() => this.model.SendMessage();

    internal void SetScriptToEdit(MonoScript monoScript) => this.model.Attach((UnityEngine.Object) monoScript);
  }
}
