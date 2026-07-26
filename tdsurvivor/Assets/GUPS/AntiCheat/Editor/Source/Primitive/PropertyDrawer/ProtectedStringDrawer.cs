// System
using System;

// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedString"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedString"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedString), true)]
    public class ProtectedStringDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            String var_Value = UnityEditor.EditorGUI.TextField(_Position, _Label, var_FakeValue.stringValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.stringValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
