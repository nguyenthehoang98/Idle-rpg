// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedFloat"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedFloat"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedFloat), true)]
    public class ProtectedFloatDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            float var_Value = UnityEditor.EditorGUI.FloatField(_Position, _Label, var_FakeValue.floatValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.floatValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
