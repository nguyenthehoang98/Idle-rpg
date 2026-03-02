using _Game.Battle.AbilitySystem;
using UnityEditor;
using UnityEngine;

namespace _Game.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(TrajectoryArg))]
    public class TrajectoryDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle coreStyle = new GUIStyle(EditorStyles.boldLabel);
            coreStyle.fontSize = 20;
            coreStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("TRAJECTORY", coreStyle);
            
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProp);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("delayStart"));

            TrajectoryType type = (TrajectoryType) typeProp.enumValueIndex;
            switch (type)
            {
                case TrajectoryType.Teleport:
                    break;
                case TrajectoryType.Path:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("path"));
                    break;
                case TrajectoryType.Velocity:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("velocity"));
                    break;
            }
        }
    }

    [CustomPropertyDrawer(typeof(VelocityArg))]
    public class VelocityDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle coreStyle = new GUIStyle(EditorStyles.boldLabel);
            coreStyle.fontSize = 14;
            coreStyle.alignment = TextAnchor.MiddleLeft;
            coreStyle.normal.textColor = new Color(1, 0, 0, 0.35f);
            EditorGUILayout.LabelField("Velocity", coreStyle);
            
            EditorGUILayout.PropertyField(property.FindPropertyRelative("acceleration"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("speed"));
        }
    }
}