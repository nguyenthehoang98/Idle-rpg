using System;
using System.Collections.Generic;
using System.Linq;
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
    private Button attachButton;
    private Button sendButton;
    private List<string> attachments = new List<string>();

    private ListView attachmentList;
    private TextField requestField;
    private VisualElement mainAreaPanel;
    private VisualElement historyPanel;
    private bool isHistoryOpen = false;
    private bool isRunning = false;

    private List<string> messages = new List<string>()
    {
        "Hello 👋",
        "How can I help you?",
        "This is a test message",
        "UI Toolkit is working 🚀"
    };

    [MenuItem("Tools/CodeBuddy Window")]
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
        attachButton = root.Q<Button>("m_attachButton");

        SetupListView();
        BindEvents();
        
        root.RegisterCallback<GeometryChangedEvent>(evt => InitLayout());
    }

    private void SetupListView()
    {
        // todo: messagesList
        messagesList.makeItem = () =>
        {
            var label = new Label();
            label.AddToClassList("chat-message");
            return label;
        };

        messagesList.bindItem = (element, index) =>
        {
            var label = element as Label;
            label.text = messages[index];
        };

        messagesList.itemsSource = messages;
        
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
            messages.Clear();
            messages.Add("New chat started ✨");
            messagesList.Rebuild();
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
            UpdateButtonStatus();
        };

        stopButton.clicked += () =>
        {
            isRunning = false;
            UpdateButtonStatus();
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

        UpdateButtonStatus();
    }

    private void UpdateButtonStatus()
    {
        stopButton.SetEnabled(isRunning);
        sendButton.SetEnabled(!isRunning);
        attachButton.SetEnabled(!isRunning);
        requestField.SetEnabled(!isRunning);
    }
}