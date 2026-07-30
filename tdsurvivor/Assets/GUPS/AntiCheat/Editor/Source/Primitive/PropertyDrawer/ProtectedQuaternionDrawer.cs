// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Property drawer for <see cref="GUPS.AntiCheat.Protected.ProtectedQuaternion"/>. Rendered as a Vector4 (x, y, z, w).
    /// </summary>
    /// <seealso cref="GUPS.AntiCheat.Protected.ProtectedQuaternion"/>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Protected.ProtectedQuaternion), true)]
    public class ProtectedQuaternionDrawer : ProtectedPropertyDrawer
    {
        /// <inheritdoc cref="ProtectedPropertyDrawer.OnGUIProperty"/>
        protected override void OnGUIProperty(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();

            // The protected wrapper serializes its raw value into the "fakeValue" backing field.
            SerializedProperty var_FakeValue = _Property.FindPropertyRelative("fakeValue");

            // EditorGUI has no QuaternionField; round-trip via Vector4 so all four components stay editable.
            Vector4 var_Value = UnityEditor.EditorGUI.Vector4Field(_Position, _Label, Helper_QuaternionToVector4(var_FakeValue.quaternionValue));

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                var_FakeValue.quaternionValue = Helper_Vector4ToQuaternion(var_Value);

                _Property.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Converts a Vector4 to a Quaternion component-wise.
        /// </summary>
        /// <param name="_Value">Source value.</param>
        /// <returns>The corresponding quaternion.</returns>
        private static Quaternion Helper_Vector4ToQuaternion(Vector4 _Value)
        {
            return new Quaternion(_Value.x, _Value.y, _Value.z, _Value.w);
        }

        /// <summary>
        /// Converts a Quaternion to a Vector4 component-wise.
        /// </summary>
        /// <param name="_Value">Source value.</param>
        /// <returns>The corresponding vector.</returns>
        private static Vector4 Helper_QuaternionToVector4(Quaternion _Value)
        {
            return new Vector4(_Value.x, _Value.y, _Value.z, _Value.w);
        }
    }
}
