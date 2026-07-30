// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedVector2"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedVector2"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedVector2), true)]
    public class ProtectedVector2Drawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            Vector2 var_Value = UnityEditor.EditorGUI.Vector2Field(_Position, _Label, var_FakeValue.vector2Value);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.vector2Value = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
