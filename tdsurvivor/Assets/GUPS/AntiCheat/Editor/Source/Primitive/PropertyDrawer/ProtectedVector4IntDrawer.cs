// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedVector4Int"/>. Rendered as a Vector4 (no Vector4IntField exists).
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedVector4Int"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedVector4Int), true)]
    public class ProtectedVector4IntDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            Vector4 var_Value = UnityEditor.EditorGUI.Vector4Field(_Position, _Label, var_FakeValue.vector4Value);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.vector4Value = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
