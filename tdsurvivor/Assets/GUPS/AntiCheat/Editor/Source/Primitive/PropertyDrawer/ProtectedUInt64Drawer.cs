// System
using System;

// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedUInt64"/>. Rendered as a signed long in the inspector.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedUInt64"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedUInt64), true)]
    public class ProtectedUInt64Drawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            Int64 var_Value = UnityEditor.EditorGUI.LongField(_Position, _Label, var_FakeValue.longValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.longValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
