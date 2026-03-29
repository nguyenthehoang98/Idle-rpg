using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeBuddy
{
    internal class ChatMessageComponent : VisualElement
    {
        private string _messageContent = "";
        private List<UnityEngine.Object> _attachments = new List<UnityEngine.Object>();
        private VisualElement _loadingIcon;
        private bool _isRotating = false;
        private double prevTime = 0.0;
        private float angle = 0.0f;

        public string MessageContent
        {
            get => _messageContent;
            set
            {
                _messageContent = value ?? "";
                UpdateMessageDisplay();
            }
        }

        public List<UnityEngine.Object> Attachments
        {
            get => _attachments;
            set
            {
                _attachments = value ?? new List<UnityEngine.Object>();
                UpdateMessageDisplay();
            }
        }

        public event Action<string, string> SaveClicked;
        public event Action<string, string, GameObject> SaveAndAddClicked;
        public event Action<string, string> UpdateClicked;

        public ChatMessageComponent()
        {
            UpdateMessageDisplay();
        }

        private void StartLoadingIconAnimation()
        {
            if (_loadingIcon == null)
            {
                var content = EditorGUIUtility.IconContent("Loading");
                var tex = content.image as Texture2D;

                _loadingIcon = new VisualElement();
                _loadingIcon.style.backgroundImage = new StyleBackground(tex);
                _loadingIcon.style.width = 30;
                _loadingIcon.style.height = 30;
                _loadingIcon.style.marginTop = 3;
                _loadingIcon.style.marginBottom = 3;
            }

            Add(_loadingIcon);

            if (_isRotating) return;

            _isRotating = true;
            prevTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += RotateLoadingIcon;
        }

        private void RotateLoadingIcon()
        {
            double current = EditorApplication.timeSinceStartup;
            double delta = current - prevTime;
            prevTime = current;

            angle += (float)(delta * 360f);
            if (angle > 360f) angle %= 360f;

            if (_loadingIcon != null)
                _loadingIcon.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
        }

        private void StopLoadingIconAnimation()
        {
            if (!_isRotating) return;

            _isRotating = false;
            EditorApplication.update -= RotateLoadingIcon;

            _loadingIcon?.RemoveFromHierarchy();
        }

        private void UpdateMessageDisplay()
        {
            Clear();

            if (string.IsNullOrEmpty(_messageContent) && (_attachments == null || _attachments.Count == 0))
            {
                StartLoadingIconAnimation();
                return;
            }

            StopLoadingIconAnimation();

            if (!string.IsNullOrEmpty(_messageContent))
            {
                ParseMessage(_messageContent);
            }

            if (_attachments != null && _attachments.Count > 0)
            {
                var panel = new VisualElement();
                panel.style.flexDirection = FlexDirection.Row;
                panel.style.flexWrap = Wrap.Wrap;
                panel.style.marginTop = 10;

                foreach (var attachment in _attachments)
                {
                    if (attachment == null) continue;

                    var field = new ObjectField
                    {
                        value = attachment
                    };

                    field.style.height = 22;
                    panel.Add(field);
                }

                Add(panel);
            }
        }

        private void ParseMessage(string message)
        {
            var lines = message.Split('\n');
            bool inCode = false;

            string code = "";
            string codeType = "";
            string textBuffer = "";

            TextField codeField = null;

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("```"))
                {
                    if (inCode)
                    {
                        CreateCodeBlock(code, codeType);
                        code = "";
                        codeType = "";
                        inCode = false;
                        codeField = null;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(textBuffer))
                        {
                            AddTextLabel(textBuffer);
                            textBuffer = "";
                        }

                        inCode = true;
                        codeType = line.Substring(3).Trim();

                        codeField = new TextField
                        {
                            isReadOnly = true
                        };

                        codeField.style.flexGrow = 1;
                        codeField.style.whiteSpace = WhiteSpace.Normal;

                        Add(codeField);
                    }
                }
                else if (inCode)
                {
                    code += line + "\n";
                    if (codeField != null)
                        codeField.value = code.Trim();
                }
                else
                {
                    textBuffer += line + "\n";
                }
            }

            if (!string.IsNullOrEmpty(textBuffer))
                AddTextLabel(textBuffer);
        }

        private void CreateCodeBlock(string codeText, string codeType)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.RowReverse;

            var copyBtn = new Button(() => GUIUtility.systemCopyBuffer = codeText)
            {
                text = "Copy"
            };
            container.Add(copyBtn);

            var saveBtn = new Button(() => SaveCode(codeText, codeType))
            {
                text = "Save"
            };
            container.Add(saveBtn);

            if (Selection.activeGameObject != null)
            {
                var addBtn = new Button(() => SaveAndAdd(codeText, codeType))
                {
                    text = "Save & Add"
                };
                container.Add(addBtn);
            }

            Add(container);
        }

        private void AddTextLabel(string textContent)
        {
            string pattern = "(http[s]?://[^\\s]+)";
            var parts = Regex.Split(textContent, pattern);

            foreach (var part in parts)
            {
                if (Regex.IsMatch(part, pattern))
                {
                    var link = new Label(part);
                    link.tooltip = part;
                    link.AddToClassList("message-link");

                    link.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        if (evt.button == 0)
                            Application.OpenURL(part);
                    });

                    Add(link);
                }
                else
                {
                    var label = new Label(MarkdownToRichConverter(part.Trim()));
                    label.enableRichText = true;
                    label.style.whiteSpace = WhiteSpace.Normal;
                    Add(label);
                }
            }
        }

        private void SaveCode(string code, string type)
        {
            SaveClicked?.Invoke(code, type);
        }

        private void SaveAndAdd(string code, string type)
        {
            SaveAndAddClicked?.Invoke(code, type, Selection.activeGameObject);
        }

        private void UpdateCode(string code, string path)
        {
            UpdateClicked?.Invoke(code, path);
        }

        private string MarkdownToRichConverter(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return "";

            markdown = Regex.Replace(markdown, "\\*\\*([^*]+)\\*\\*", "<b>$1</b>");
            markdown = Regex.Replace(markdown, "__([^_]+)__", "<i>$1</i>");
            markdown = Regex.Replace(markdown, "`([^`]+)`", "<b><i>$1</i></b>");

            return markdown;
        }

        public new class UxmlFactory : UxmlFactory<ChatMessageComponent, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _messageContent =
                new UxmlStringAttributeDescription { name = "message-content", defaultValue = "" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((ChatMessageComponent)ve).MessageContent = _messageContent.GetValueFromBag(bag, cc);
            }
        }
    }
}