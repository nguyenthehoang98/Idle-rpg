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
            EditorGUILayout.PropertyField(property);
        }
    }
}