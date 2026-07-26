// System
using System;

// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedInt32"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedInt32"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedInt32), true)]
    public class ProtectedInt32Drawer : ProtectedPropertyDrawer
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
