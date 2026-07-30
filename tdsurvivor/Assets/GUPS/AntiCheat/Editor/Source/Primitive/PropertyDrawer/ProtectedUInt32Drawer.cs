// System
using System;

// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedUInt32"/>. Rendered as a signed int in the inspector.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedUInt32"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedUInt32), true)]
    public class ProtectedUInt32Drawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            Int32 var_Value = UnityEditor.EditorGUI.IntField(_Position, _Label, var_FakeValue.intValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.intValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
