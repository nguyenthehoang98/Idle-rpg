using UnityEditor;
using UnityEngine;

namespace _Game.AbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(CoreAbilityArg))]
    public class CoreAbilityDrawProperty : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.LabelField("CORE");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("lifeTime"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("bulletPrefab"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("muzzleOffsetPosition"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("animationName"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("maxCollision"));
        }
    }
}