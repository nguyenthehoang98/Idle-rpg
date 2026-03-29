using System.Collections.Generic;
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
    private Label chatNameLabel;

    private VisualElement mainAreaPanel;
    private VisualElement historyPanel;
    private bool isHistoryOpen = false;

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
        chatNameLabel = root.Q<Label>("m_chatNameLabel");
        
        mainAreaPanel = root.Q<VisualElement>("m_mainAreaPanel");
        historyPanel = mainAreaPanel.Q<VisualElement>("m_historyPanel");

        SetupListView();
        BindEvents();
        
        root.RegisterCallback<GeometryChangedEvent>(evt => InitLayout());
    }

    private void SetupListView()
    {
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
    }

    private void InitLayout()
    {
        float width = historyPanel.resolvedStyle.width;
        historyPanel.style.left = -width; 
        
        var root = rootVisualElement;
        float height = root.resolvedStyle.height;
        mainAreaPanel.style.height = height - 100;
    }
}