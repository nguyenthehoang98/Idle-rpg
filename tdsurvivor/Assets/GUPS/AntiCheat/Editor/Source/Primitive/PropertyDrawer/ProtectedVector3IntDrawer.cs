// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedVector3Int"/>.
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedVector3Int"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedVector3Int), true)]
    public class ProtectedVector3IntDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            Vector3Int var_Value = UnityEditor.EditorGUI.Vector3IntField(_Position, _Label, var_FakeValue.vector3IntValue);

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.vector3IntValue = var_Value;

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
