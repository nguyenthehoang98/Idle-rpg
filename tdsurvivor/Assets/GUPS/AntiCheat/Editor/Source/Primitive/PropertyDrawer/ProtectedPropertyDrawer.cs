// Unity
using UnityEditor;
using UnityEngine;

namespace GUPS.AntiCheat.Editor
{
    /// <summary>
    /// Base property drawer for protected primitive fields. Wraps the editor field in
    /// <see cref="EditorGUI.BeginProperty"/> / <see cref="EditorGUI.EndProperty"/> and forces indent to zero,
    /// then delegates the actual field rendering to <see cref="OnGUIProperty"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(GUPS.AntiCheat.Core.Protected.IProtected))]
    public class ProtectedPropertyDrawer : PropertyDrawer
    {
        /// <inheritdoc cref="UnityEditor.PropertyDrawer.OnGUI"/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // BeginProperty / EndProperty on the parent property keeps prefab override logic on the whole field.
            EditorGUI.BeginProperty(position, label, property);

            // Child fields would otherwise be indented twice; reset and restore around the field draw.
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var amountRect = new Rect(position.x, position.y, position.width, position.height);

            this.OnGUIProperty(amountRect, property, label);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Renders the editor field for the protected value. Overridden by each concrete drawer.
        /// </summary>
        /// <param name="position">Rect to draw the field in.</param>
        /// <param name="property">The serialized property representing the protected value.</param>
        /// <param name="label">Inspector label.</param>
        protected virtual void OnGUIProperty(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }
}