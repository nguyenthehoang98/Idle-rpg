using UnityEditor;
using UnityEngine;

namespace _Game.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(TrajectoryArg))]
    public class TrajectoryDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.LabelField("TRAJECTORY");
            
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProp);

            TrajectoryType type = (TrajectoryType) typeProp.enumValueIndex;
            switch (type)
            {
                case TrajectoryType.None:
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
}