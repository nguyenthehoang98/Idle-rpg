// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedVector3"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedVector3"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedVector3), true)]
    public class ProtectedVector3Drawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            Vector3 var_Value = UnityEditor.EditorGUI.Vector3Field(_Position, _Label, var_FakeValue.vector3Value);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.vector3Value = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
