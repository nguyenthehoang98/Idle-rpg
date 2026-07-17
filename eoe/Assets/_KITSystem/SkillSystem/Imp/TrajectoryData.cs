using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _KITSystem.SkillSystem.Imp
{
    [Serializable]
    public struct TrajectoryData
    {
        public TrajectoryType type;
        public AnimationCurve projectileCurve;
        public AnimationCurve boomerangInitCurve;
        public AnimationCurve boomerangReturnCurve;
    }

    public enum TrajectoryType
    {
        Projectile = 1,
        Boomerang = 2,
        Stationary = 3,
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(TrajectoryData))]
    public class TrajectoryDataPropertyDrawer : PropertyDrawer
    {
        const float Padding = 6f;
        const float Space = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var type = property.FindPropertyRelative(nameof(TrajectoryData.type));

            var projectileCurve = property.FindPropertyRelative(nameof(TrajectoryData.projectileCurve));

            var boomerangInitCurve = property.FindPropertyRelative(nameof(TrajectoryData.boomerangInitCurve));
            var boomerangReturnCurve = property.FindPropertyRelative(nameof(TrajectoryData.boomerangReturnCurve));
            
            GUI.Box(position, GUIContent.none);

            Rect rect = new Rect(
                position.x + Padding,
                position.y + Padding,
                position.width - Padding * 2,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(rect, type);

            rect.y += EditorGUIUtility.singleLineHeight + Space;

            var trajectoryType = (TrajectoryType)type.intValue;

            switch (trajectoryType)
            {
                case TrajectoryType.Projectile:
                    Draw(ref rect, projectileCurve);
                    break;

                case TrajectoryType.Boomerang:
                    Draw(ref rect, boomerangInitCurve);
                    Draw(ref rect, boomerangReturnCurve);
                    break;
            }

            EditorGUI.EndProperty();
        }

        void Draw(ref Rect rect, SerializedProperty property)
        {
            float h = EditorGUI.GetPropertyHeight(property, true);
            rect.height = h;
            EditorGUI.PropertyField(rect, property, true);
            rect.y += h + Space;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = Padding * 2;
            h += EditorGUIUtility.singleLineHeight + Space;

            var type = (TrajectoryType)property.FindPropertyRelative(nameof(TrajectoryData.type)).intValue;

            switch (type)
            {
                case TrajectoryType.Projectile:
                    h += EditorGUI.GetPropertyHeight(
                        property.FindPropertyRelative(nameof(TrajectoryData.projectileCurve)), true);
                    break;

                case TrajectoryType.Boomerang:
                    h += EditorGUI.GetPropertyHeight(
                        property.FindPropertyRelative(nameof(TrajectoryData.boomerangInitCurve)), true);

                    h += EditorGUI.GetPropertyHeight(
                        property.FindPropertyRelative(nameof(TrajectoryData.boomerangReturnCurve)), true);

                    break;
            }

            return h + Space * 5;
        }
    }
#endif
}