using UnityEditor;
using UnityEngine;

namespace _Game.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(CoreAbilityArg))]
    public class CoreAbilityDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle coreStyle = new GUIStyle(EditorStyles.boldLabel);
            coreStyle.fontSize = 20;
            coreStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("CORE", coreStyle);

            EditorGUILayout.PropertyField(property.FindPropertyRelative("lifeTime"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("bulletPrefab"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("muzzleOffsetPosition"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("animationName"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("findTarget"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("maxCollision"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("shouldResetCollision"));
            if(property.FindPropertyRelative("shouldResetCollision").boolValue)    
                EditorGUILayout.PropertyField(property.FindPropertyRelative("resetCollisionInterval"));
        }
    }
}