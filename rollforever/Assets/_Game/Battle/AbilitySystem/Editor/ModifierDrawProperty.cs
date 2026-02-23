using _Game.Battle.AbilitySystem;
using UnityEditor;
using UnityEngine;

namespace _Game.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(StateModifierArg))]
    public class ModifierDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle coreStyle = new GUIStyle(EditorStyles.boldLabel);
            coreStyle.fontSize = 20;
            coreStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("MODIFIER", coreStyle);
            
            EditorGUILayout.PropertyField(property.FindPropertyRelative("type"));

            StateModifierType type = (StateModifierType) property.FindPropertyRelative("type").enumValueIndex;
            switch (type)
            {
                case StateModifierType.None:
                    break;
                case StateModifierType.Knockback:
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("group"));
                    property.FindPropertyRelative("group").enumValueIndex = (int) (ModifierGroup.Target);
                    GUI.enabled = true;
                    var valueProp = property.FindPropertyRelative("value");
                    valueProp.floatValue = EditorGUILayout.FloatField("Knockback distance", valueProp.floatValue);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("duration"));
                    break;
                case StateModifierType.Stun:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("group"));
                    if (property.FindPropertyRelative("group").enumValueIndex == (int) ModifierGroup.Teammate)
                    {
                        property.FindPropertyRelative("group").enumValueIndex = (int) (ModifierGroup.Target);
                    }

                    EditorGUILayout.PropertyField(property.FindPropertyRelative("duration"));

                    break;
                default:
                    EditorGUILayout.HelpBox($"Modifier {type} chưa được định nghĩa", MessageType.Error);
                    break;
            }
        }
    }
}