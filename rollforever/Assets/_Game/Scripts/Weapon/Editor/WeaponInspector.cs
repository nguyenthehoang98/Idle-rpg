using UnityEditor;
using UnityEngine;

namespace _Game.Scripts.Weapon.Editor
{
    [CustomEditor(typeof(WeaponSO))]
    public class WeaponInspector : UnityEditor.Editor
    {
        SerializedProperty weaponIconProp;

        private void OnEnable()
        {
            weaponIconProp = serializedObject.FindProperty("weaponIcon");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(10);
            GUILayout.Label("Weapon Icon", EditorStyles.boldLabel);

            float size = Mathf.Min(EditorGUIUtility.currentViewWidth - 40, 150);
            Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));

            DrawSpritePicker(rect);

            GUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawSpritePicker(Rect rect)
        {
            Event e = Event.current;

            // Background
            EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));

            Sprite sprite = weaponIconProp.objectReferenceValue as Sprite;

            // Draw sprite
            if (sprite != null)
            {
                Texture2D tex = sprite.texture;
                Rect spriteRect = sprite.textureRect;

                Rect uv = new Rect(
                    spriteRect.x / tex.width,
                    spriteRect.y / tex.height,
                    spriteRect.width / tex.width,
                    spriteRect.height / tex.height
                );

                GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
            }
            else
            {
                GUI.Label(rect, "Drag Sprite Here\nor Click to Pick",
                    new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter
                    });
            }

            // Click to open picker
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                EditorGUIUtility.ShowObjectPicker<Sprite>(
                    weaponIconProp.objectReferenceValue,
                    false,
                    "",
                    1234);
                e.Use();
            }

            // Handle picker selection
            if (e.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == 1234)
                {
                    weaponIconProp.objectReferenceValue =
                        EditorGUIUtility.GetObjectPickerObject();
                }
            }

            // Drag & Drop
            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.DragUpdated)
                {
                    if (DragAndDrop.objectReferences.Length > 0 &&
                        DragAndDrop.objectReferences[0] is Sprite)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }
                }

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    weaponIconProp.objectReferenceValue =
                        DragAndDrop.objectReferences[0];
                    e.Use();
                }
            }
        }
    }
}