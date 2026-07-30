// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedBool"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedBool"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedBool), true)]
    public class ProtectedBoolDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            bool var_Value = UnityEditor.EditorGUI.Toggle(_Position, _Label, var_FakeValue.boolValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.boolValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
