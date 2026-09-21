#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Chidwi.MinMaxSliderAttribute
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    internal sealed class MinMaxSliderDrawer : PropertyDrawer
    {
        private const float LabelWidth = 100f;
        private const float FieldWidth = 40f;
        private const float MinimumSliderWidth = 100f;
        private const float FieldToSliderPadding = 5f;
        private const float FieldPadding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                EditorGUI.LabelField(position, label.text, "Use MinMaxSlider with Vector2.");
                return;
            }

            var slider = (MinMaxSliderAttribute)attribute;
            float totalWidth = position.width - LabelWidth - FieldWidth * 2f -
                               FieldToSliderPadding * 2f - FieldPadding * 3f;
            float sliderWidth = Mathf.Max(totalWidth, MinimumSliderWidth);
            var labelRect = new Rect(position.x, position.y, LabelWidth, position.height);
            var minFieldRect = new Rect(position.x + LabelWidth + FieldPadding, position.y, FieldWidth, position.height);
            float sliderX = minFieldRect.x + FieldWidth + FieldToSliderPadding;
            var sliderRect = new Rect(sliderX, position.y, sliderWidth, position.height);
            var maxFieldRect = new Rect(sliderRect.x + sliderRect.width + FieldToSliderPadding, position.y, FieldWidth, position.height);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(labelRect, label);

            Vector2 range = property.vector2Value;
            float min = EditorGUI.FloatField(minFieldRect, range.x);
            float max = EditorGUI.FloatField(maxFieldRect, range.y);
            min = Mathf.Clamp(min, slider.min, max);
            max = Mathf.Clamp(max, min, slider.max);
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, slider.min, slider.max);

            if (GUI.changed)
            {
                property.vector2Value = new Vector2(min, max);
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
