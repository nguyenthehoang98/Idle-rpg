
namespace AI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using AI;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEngine;

    public class AIGenerateImageEditorWindow : AIBash
    {
        #region State

        private string content = "";
        private List<Texture2D> inputImages = new List<Texture2D>();

        private List<string> responses = new List<string>();

        private float fakeProgress;
        private double fakeProgressLastTime;
        
        private double currentTime;
        private double runningLastTime;
        
        private Vector2 scrollPos;
        private bool isRunning;
        private List<Texture2D> generatedImages = new List<Texture2D>();
        private EditorCoroutine coroutine;

        #endregion

        [MenuItem("Tools/AI/Generate image")]
        public static void ShowWindow()
        {
            GetWindow<AIGenerateImageEditorWindow>("Generate image");
        }

        #region GUI

        private void OnGUI()
        {
            DrawInputSection();
            DrawProgress();
            DrawButtons();
            DrawResponses();
        }

        private void DrawInputSection()
        {
            GUILayout.Label("Content:");
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            content = EditorGUILayout.TextArea(content, style, GUILayout.Height(80));

            GUILayout.Space(10);

            GUILayout.Label("Images:");

            int removeIndex = -1;

            for (int i = 0; i < inputImages.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                inputImages[i] = (Texture2D)EditorGUILayout.ObjectField(inputImages[i], typeof(Texture2D), false);

                if (GUILayout.Button("X", GUILayout.Width(30)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
                inputImages.RemoveAt(removeIndex);

            if (GUILayout.Button("Add Image"))
            {
                inputImages.Add(null);
            }
        }

        private void DrawButtons()
        {
            GUILayout.Space(10);

            GUI.enabled = !isRunning;

            if (GUILayout.Button("Generate Prompt"))
            {
                StartCoroutine(GeneratePrompt());
            }

            GUI.enabled = responses.Count > 0;

            if (GUILayout.Button("Generate All Images (Fake)"))
            {
                GenerateAllImages();
            }

            GUI.enabled = true;
        }
        
        private void DrawProgress()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Last time running: " + TimeSpan.FromSeconds(currentTime - runningLastTime));
        
            if (!isRunning) return;

            currentTime = EditorApplication.timeSinceStartup;
        
            UpdateFakeProgress();

            var rect = GUILayoutUtility.GetRect(200, 20);
            EditorGUI.ProgressBar(rect, fakeProgress, "Processing...");

            Repaint();
        }
        
        private void UpdateFakeProgress()
        {
            var time = EditorApplication.timeSinceStartup;
            var delta = time - fakeProgressLastTime;
            fakeProgressLastTime = time;

            fakeProgress += (float)(delta * 0.5f);
            if (fakeProgress > 1f) fakeProgress = 0f;
        }

        private void DrawResponses()
        {
            GUILayout.Space(10);
            GUILayout.Label("Responses:");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < responses.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // LEFT: PROMPT
                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField($"Prompt {i + 1}");

                responses[i] = EditorGUILayout.TextArea(responses[i], GUILayout.Height(108));

                if (GUILayout.Button("Generate Image"))
                {
                    GenerateImageFake(i, responses[i]);
                }

                EditorGUILayout.EndVertical();

                // RIGHT: IMAGE PREVIEW
                EditorGUILayout.BeginVertical(GUILayout.Width(140));

                Texture2D tex = generatedImages[i];
                
                EditorGUILayout.Space(10);

                if (tex != null)
                {
                    GUILayout.Label(tex, GUILayout.Width(140), GUILayout.Height(140));
                }
                else
                {
                    GUILayout.Box("No Image", GUILayout.Width(140), GUILayout.Height(140));
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Core Logic

        private void StartCoroutine(IEnumerator routine)
        {
            StopCurrentCoroutine();
            coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(routine);
        }

        private void StopCurrentCoroutine()
        {
            if (coroutine != null)
                EditorCoroutineUtility.StopCoroutine(coroutine);
        }

        private IEnumerator GeneratePrompt()
        {
            responses.Clear();
            generatedImages.Clear();
            isRunning = true;
            runningLastTime = EditorApplication.timeSinceStartup;
            responses.Clear();

            string prompt = BuildPrompt();

            yield return SendRequest(prompt, tuple =>
            {
                if (tuple.success)
                {
                    // giả sử trả về nhiều dòng → split
                    var lines = tuple.response.Split('\n');

                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            responses.Add(line);
                            generatedImages.Add(null);
                        }
                    }
                }
                else
                {
                    responses.Add("ERROR: " + tuple.response);
                }
            });

            isRunning = false;
        }

        private string BuildPrompt()
        {
            return $@"
You are an AI that generates image prompts.

Input:
- Content: {content}
- Number of images: {inputImages.Count}

Requirements:
- Generate multiple image prompts
- Each prompt should be 1 line
- No explanation
- Focus on style, lighting, color

Output:
prompt 1
prompt 2
prompt 3
";
        }

        #endregion

        #region Fake Image Generation

        private void GenerateImageFake(int index, string prompt)
        {
            Texture2D tex = new Texture2D(128, 128);

            Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    tex.SetPixel(x, y, randomColor);
                }
            }

            tex.Apply();

            generatedImages[index] = tex;
        }

        private void GenerateAllImages()
        {
            for (var i = 0; i < responses.Count; i++)
            {
                GenerateImageFake(i, responses[i]);
            }
        }

        #endregion

        protected override string Model => "qwen3:4b";
    }
}