// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedDecimal"/>. Rendered as a double in the inspector.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedDecimal"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedDecimal), true)]
    public class ProtectedDecimalDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            double var_Value = UnityEditor.EditorGUI.DoubleField(_Position, _Label, var_FakeValue.doubleValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.doubleValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
