using Geometry;
using Geometry.Primary;
using UnityEditor;
using UnityEngine;

namespace _Game.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(Shape))]
    public class ShapeDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.LabelField("SHAPE");
            
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProp);
        
            ShapeType type = (ShapeType) typeProp.enumValueIndex;
            switch (type)
            {
                case ShapeType.Box:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("size"));
                    break;
                case ShapeType.Circle:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
                    break;
            }
        }
    }
}