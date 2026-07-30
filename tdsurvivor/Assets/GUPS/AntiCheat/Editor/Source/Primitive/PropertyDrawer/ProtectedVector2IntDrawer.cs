// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedVector2Int"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedVector2Int"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedVector2Int), true)]
    public class ProtectedVector2IntDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            Vector2Int var_Value = UnityEditor.EditorGUI.Vector2IntField(_Position, _Label, var_FakeValue.vector2IntValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.vector2IntValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
