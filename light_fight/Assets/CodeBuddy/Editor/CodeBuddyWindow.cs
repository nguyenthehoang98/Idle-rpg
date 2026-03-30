using System;
using System.Collections.Generic;
using System.Linq;
using _KIT.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CodeBuddyWindow : EditorWindow
{
    private VisualTreeAsset uxml;
    private StyleSheet mainStyle;
    private StyleSheet chatStyle;

    private ListView messagesList;
    private Button newChatButton;
    private Button historyButton;
    private Button stopButton;
    private Button sendButton;
    private List<string> attachments = new List<string>();

    private ListView attachmentList;
    private TextField requestField;
    private VisualElement mainAreaPanel;
    private VisualElement historyPanel;
    private bool isHistoryOpen = false;
    private bool isRunning = false;

    private ChatDatabase chatDatabase;
    private ChatConversation currentConversation;

    [MenuItem("Tools/AI/Window")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<CodeBuddyWindow>();
        wnd.titleContent = new GUIContent("CodeBuddy");
    }

    public void CreateGUI()
    {
        // Load UXML + USS
        uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CodeBuddy/Editor/Layouts/CodeWindow.uxml");
        mainStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CodeBuddy/Editor/Layouts/CodeWindow.uss");
        chatStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CodeBuddy/Editor/Layouts/ChatMessageStyles.uss");

        // Clone UI
        var root = rootVisualElement;
        root.Clear();

        var tree = uxml.CloneTree();
        root.Add(tree);

        // Add styles
        root.styleSheets.Add(mainStyle);
        root.styleSheets.Add(chatStyle);

        // Bind elements
        messagesList = root.Q<ListView>("m_messagesList");
        newChatButton = root.Q<Button>("m_newChatButton");
        historyButton = root.Q<Button>("m_historyButton");
        mainAreaPanel = root.Q<VisualElement>("m_mainAreaPanel");
        historyPanel = root.Q<VisualElement>("m_historyPanel");
        requestField = root.Q<TextField>("m_requestField");
        attachmentList = root.Q<ListView>("m_attachmentList");
        stopButton = root.Q<Button>("m_stopButton");
        sendButton = root.Q<Button>("m_sendButton");
        
        root.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            InitLayout();
            SetupListView();
            BindEvents();
        });
    }

    private void SetupListView()
    {
        // todo: messagesList
        messagesList.makeItem = () =>
        {
            var container = new VisualElement();

            var label = new Label();
            label.name = "content"; // 👈 BẮT BUỘC phải có
            label.AddToClassList("chat-message");
            container.Add(label);

            return label;
        };

        messagesList.bindItem = (element, index) =>
        {
            var msg = currentConversation.messages[index];
            var label = element.Q<Label>("content");
            if (msg != null && label != null)
            {
                label.text = msg.content;
                element.EnableInClassList("user", msg.role == "user");
                element.EnableInClassList("assistant", msg.role == "assistant");
            }
        };
        
        // todo: attachmentList
        attachmentList.makeItem = () =>
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.position = Position.Relative;
            container.AddToClassList("attachment-item");

            var label = new Label();
            label.name = "name";

            var removeBtn = new Button(() => { });
            removeBtn.style.position = Position.Absolute;
            removeBtn.style.right = 4;
            removeBtn.style.top = 2;
            removeBtn.text = "X";
            removeBtn.name = "remove";

            container.Add(label);
            container.Add(removeBtn);

            return container;
        };
        
        attachmentList.bindItem = (element, index) =>
        {
            Label label = element.Q<Label>("name");
            label.text = attachments[index];

            Button removeBtn = element.Q<Button>("remove");
            int capturedIndex = index;
            
            removeBtn.clicked += () =>
            {
                attachments.RemoveAt(capturedIndex);
                attachmentList.Rebuild();
            };
        };
        
        attachmentList.itemsSource = attachments;
    }

    private void BindEvents()
    {
        newChatButton.clicked += () =>
        {
            currentConversation = new ChatConversation(TimeUtils.GetUnixTime(TimeUtils.Now));
            
            chatDatabase.conversations.Add(currentConversation.id);

            SaveChatConversation(currentConversation);
            SaveChatDatabase(chatDatabase);
        };

        historyButton.clicked += () =>
        {
            isHistoryOpen = !isHistoryOpen;

            if (isHistoryOpen)
            {
                historyPanel.style.left = 0;
            }
            else
            {
                float width = historyPanel.resolvedStyle.width;
                historyPanel.style.left = -width;
            }
        };

        sendButton.clicked += () =>
        {
            isRunning = true;
            SendMessage(requestField.text);
            RefreshUI();
        };

        stopButton.clicked += () =>
        {
            isRunning = false;
            RefreshUI();
        };
        
        requestField.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        });

        requestField.RegisterCallback<DragPerformEvent>(evt =>
        {
            DragAndDrop.AcceptDrag();

            foreach (var obj in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path))
                {
                    // 👉 Folder → lấy toàn bộ file bên trong
                    string[] guids = AssetDatabase.FindAssets("", new[] { path });

                    foreach (var guid in guids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        AddAttachment(assetPath);
                    }
                }
                else
                {
                    AddAttachment(path);
                }
            }

            attachmentList.Rebuild();
        });
    }
    
    void AddAttachment(string path)
    {
        string ext = System.IO.Path.GetExtension(path).ToLower();
        if (ext == ".cs" || ext == ".asset")
        {
            if (attachments.Any(a => a == path)) return;
            attachments.Add(path);
        }
    }

    private void InitLayout()
    {
        float width = historyPanel.resolvedStyle.width;
        historyPanel.style.left = -width;

        var root = rootVisualElement;
        float height = root.resolvedStyle.height;
        mainAreaPanel.style.height = height - 100;

        chatDatabase = GetChatDatabase();
        if (chatDatabase.conversations.Count > 0)
        {
            long last = chatDatabase.conversations.Last();
            currentConversation = GetChatConversation(last);
            if (currentConversation.messages.Count == 0)
            {
                SaveChatConversation(currentConversation);
                SaveChatDatabase(chatDatabase);
            }
        }
        else
        {
            currentConversation = new ChatConversation(TimeUtils.GetUnixTime(TimeUtils.Now));
            chatDatabase.conversations.Add(currentConversation.id);
            SaveChatConversation(currentConversation);
            SaveChatDatabase(chatDatabase);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        stopButton.SetEnabled(isRunning);
        sendButton.SetEnabled(!isRunning);
        requestField.SetEnabled(!isRunning);
        
        messagesList.itemsSource = currentConversation.messages;
        messagesList.Rebuild();

        attachmentList.itemsSource = attachments;
        attachmentList.Rebuild();
    }
    
    private void OnAIResponse(string response)
    {
        var msg = new ChatMessage
        {
            role = "assistant",
            content = response,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        currentConversation.messages.Add(msg);
        SaveChatConversation(currentConversation);
        RefreshUI();
    }
    
    private void SendMessage(string text)
    {
        var msg = new ChatMessage
        {
            role = "user",
            content = text,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        currentConversation.messages.Add(msg);
        SaveChatConversation(currentConversation);
        RefreshUI();
    }
    
    void SaveDatabase(object o, string fileName)
    {
        string path = Application.dataPath + $"/Ai/{fileName}.json";
        string json = JsonUtility.ToJson(o, true);
        System.IO.File.WriteAllText(path, json);
    }
    
    (bool success, T data) LoadDatabase<T>(string fileName)
    {
        string path = Application.dataPath + $"/Ai/{fileName}.json";
        Debug.Log("Load: " + path);
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            return (true, JsonUtility.FromJson<T>(json));
        }

        return (false, default);
    }

    ChatDatabase GetChatDatabase()
    {
        (bool success, ChatDatabase data) = LoadDatabase<ChatDatabase>("chat_database");
        return success ? data : new ChatDatabase();
    }
    
    void SaveChatDatabase(ChatDatabase database) => SaveDatabase(database, "chat_database");

    ChatConversation GetChatConversation(long id)
    {
        (bool success, ChatConversation data) = LoadDatabase<ChatConversation>("chat_" + id);
        return success ? data : new ChatConversation(id);
    }
    
    void SaveChatConversation(ChatConversation conversation) => SaveDatabase(conversation, "chat_" + conversation.id);
    
    [Serializable]
    public class ChatMessage
    {
        public string role; // "user" | "assistant"
        public string content;
        public long timestamp;
    }
    
    [Serializable]
    public class ChatConversation
    {
        public long id;
        public string title;
        public string summary;
        public List<ChatMessage> messages = new List<ChatMessage>();

        public ChatConversation(long id)
        {
            this.id = id;
            Debug.Log("id: " + id);
        }
    }
    
    [Serializable]
    public class ChatDatabase
    {
        public List<long> conversations = new List<long>();
    }
}