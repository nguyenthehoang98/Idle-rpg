using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _KITSystem.SkillSystem.Imp
{
    [Serializable]
    public struct ColliderData
    {
        public ColliderType type;
        public float circleRadius;
        public Vector2 rectangleSize;
        public Vector2 relativePosition;
        public bool dependencyRelativeRotation;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ColliderData))]
    public class ColliderDataPropertyDrawer : PropertyDrawer
    {
        const float Padding = 6f;
        const float Space = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var type = property.FindPropertyRelative(nameof(ColliderData.type));
            var relativePosition = property.FindPropertyRelative(nameof(ColliderData.relativePosition));
            var dependencyRelativeRotation = property.FindPropertyRelative(nameof(ColliderData.dependencyRelativeRotation));

            GUI.Box(position, GUIContent.none);

            Rect rect = new Rect(position.x + Padding, position.y + Padding, position.width - Padding * 2, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(rect, type);

            rect.y += EditorGUIUtility.singleLineHeight + Space;

            EditorGUI.PropertyField(rect, relativePosition);

            rect.y += EditorGUIUtility.singleLineHeight + Space;

            switch ((ColliderType)type.intValue)
            {
                case ColliderType.Circle:
                    
                    var circleRadius = property.FindPropertyRelative(nameof(ColliderData.circleRadius));
                    
                    rect.height = EditorGUI.GetPropertyHeight(circleRadius, true);
                  
                    EditorGUI.PropertyField(rect, circleRadius, true);
                  
                    break;
                case ColliderType.Rectangle:

                    EditorGUI.PropertyField(rect, dependencyRelativeRotation);

                    rect.y += EditorGUIUtility.singleLineHeight + Space;
                    
                    var rectangleSize = property.FindPropertyRelative(nameof(ColliderData.rectangleSize));
                    
                    rect.height = EditorGUI.GetPropertyHeight(rectangleSize, true);
                    
                    EditorGUI.PropertyField(rect, rectangleSize, true);
                    break;
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = Padding * 2;
            h += EditorGUIUtility.singleLineHeight + Space;
            ColliderType type = (ColliderType)property.FindPropertyRelative(nameof(ColliderData.type)).intValue;

            switch (type)
            {
                case ColliderType.Circle:
                    h += EditorGUIUtility.singleLineHeight + Space;
                    h += EditorGUI.GetPropertyHeight(
                        property.FindPropertyRelative(nameof(ColliderData.circleRadius)), true);
                    break;
                case ColliderType.Rectangle:
                    h += EditorGUIUtility.singleLineHeight + Space;
                    h += EditorGUIUtility.singleLineHeight + Space;
                    h += EditorGUI.GetPropertyHeight(
                        property.FindPropertyRelative(nameof(ColliderData.rectangleSize)), true);
                    break;
            }

            return h;
        }
    }
#endif
}