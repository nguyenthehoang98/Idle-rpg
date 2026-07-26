// System
using System;

// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedInt64"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedInt64"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedInt64), true)]
    public class ProtectedInt64Drawer : ProtectedPropertyDrawer
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
