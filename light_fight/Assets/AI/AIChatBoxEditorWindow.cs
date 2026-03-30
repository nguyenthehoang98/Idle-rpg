namespace AI
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEngine;

    public class AIChatBoxEditorWindow : AIBash
    {
        #region Data

        private List<string> messages = new List<string>();
        private Vector2 scrollPos;

        private string input = "";

        private bool isRunning;
        private float fakeProgress;
        private double lastTime;

        private EditorCoroutine coroutine;

        #endregion

        [MenuItem("Tools/AI/Chat Box")]
        public static void ShowWindow()
        {
            GetWindow<AIChatBoxEditorWindow>("AI Chat");
        }

        private void OnGUI()
        {
            DrawHeader();

            DrawChatArea();

            DrawProgress();

            DrawInput();
        }

        #region UI

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                messages.Clear();
            }

            GUILayout.Label("History", EditorStyles.toolbarButton);

            GUILayout.Space(10);

            GUILayout.Label("10 Configs Fast", EditorStyles.toolbarButton);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawChatArea()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < messages.Count; i++)
            {
                DrawMessage(messages[i], i % 2 == 0);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawMessage(string msg, bool isUser)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.wordWrap = true;
            style.padding = new RectOffset(10, 10, 6, 6);

            if (isUser)
            {
                style.normal.textColor = Color.white;
            }

            EditorGUILayout.BeginHorizontal();

            if (isUser)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(msg, style, GUILayout.MaxWidth(position.width * 0.7f));
            }
            else
            {
                GUILayout.Label(msg, style, GUILayout.MaxWidth(position.width * 0.7f));
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawInput()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(40));

            GUI.SetNextControlName("ChatInput");

            input = EditorGUILayout.TextField(input, GUILayout.Height(30));

            GUI.enabled = !isRunning;

            if (GUILayout.Button("Send", GUILayout.Width(80), GUILayout.Height(30)))
            {
                SendMessage();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            // Enter to send
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                SendMessage();
                Event.current.Use();
            }
        }

        private void DrawProgress()
        {
            if (!isRunning) return;

            var rect = GUILayoutUtility.GetRect(200, 18);
            EditorGUI.ProgressBar(rect, fakeProgress, "Thinking...");

            UpdateFakeProgress();
            Repaint();
        }

        #endregion

        #region Logic

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(input)) return;

            messages.Add(input); // user

            string prompt = BuildPrompt(input);

            input = "";

            StartCoroutine(Request(prompt));
        }

        private IEnumerator Request(string prompt)
        {
            isRunning = true;
            fakeProgress = 0f;
            lastTime = EditorApplication.timeSinceStartup;

            yield return SendRequest(prompt, tuple =>
            {
                if (tuple.success)
                {
                    messages.Add(tuple.response); // AI response
                }
                else
                {
                    messages.Add("ERROR: " + tuple.response);
                }
            });

            isRunning = false;
        }

        private string BuildPrompt(string userInput)
        {
            return $@"
You are a helpful Unity assistant.

User:
{userInput}

Answer clearly and concisely.
";
        }

        private void StartCoroutine(IEnumerator routine)
        {
            if (coroutine != null)
                EditorCoroutineUtility.StopCoroutine(coroutine);

            coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(routine);
        }

        private void UpdateFakeProgress()
        {
            var time = EditorApplication.timeSinceStartup;
            var delta = time - lastTime;
            lastTime = time;

            fakeProgress += (float)(delta * 0.5f);
            if (fakeProgress > 1f) fakeProgress = 0f;
        }

        #endregion

        protected override string Model => "qwen3:4b";
    }
}