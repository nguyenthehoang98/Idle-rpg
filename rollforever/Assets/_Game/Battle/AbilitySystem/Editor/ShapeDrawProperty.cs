using _Game.Battle.AbilitySystem;
using Geometry.Primary;
using UnityEditor;
using UnityEngine;

namespace _Game.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(ShapeArg))]
    public class ShapeDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle coreStyle = new GUIStyle(EditorStyles.boldLabel);
            coreStyle.fontSize = 20;
            coreStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("SHAPE", coreStyle);

            var customProp = property.FindPropertyRelative("customValue");
            EditorGUILayout.PropertyField(customProp);
            
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProp);
        
            ShapeType type = (ShapeType) typeProp.enumValueIndex;
            if(type == ShapeType.Box)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("size"));
                if (customProp.boolValue)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("extraSize"));
                }
            }
            else if(type == ShapeType.Circle)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                if (customProp.boolValue)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("extraRadius"));
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"Shape {type} chưa được định nghĩa", MessageType.Error);
            }
            
            if (customProp.boolValue)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("curve"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("duration"));
            }
        }
    }
}