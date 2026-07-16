using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LitMotion.Editor
{
    [CustomPropertyDrawer(typeof(SerializableMotionSettings<,>))]
    public class SerializableMotionSettingsDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldout = new Foldout()
            {
                text = property.displayName,
            };

            foldout.BindProperty(property);

            Group(foldout, group =>
            {
                var valueType = fieldInfo.FieldType.GenericTypeArguments[0];
                var relative = property.FindPropertyRelative("relative");

                VisualElement relativeField;
                VisualElement startValueField;
                VisualElement endValueField;

                if (valueType == typeof(FixedString32Bytes))
                {
                    relativeField =
                        PropertyFieldHelper.CreateFixedStringField(relative, FixedString32Bytes.UTF8MaxLengthInBytes);
                    startValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("startValue"), FixedString32Bytes.UTF8MaxLengthInBytes);
                    endValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("endValue"), FixedString32Bytes.UTF8MaxLengthInBytes);
                }
                else if (valueType == typeof(FixedString64Bytes))
                {
                    relativeField =
                        PropertyFieldHelper.CreateFixedStringField(relative, FixedString64Bytes.UTF8MaxLengthInBytes);
                    startValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("startValue"), FixedString64Bytes.UTF8MaxLengthInBytes);
                    endValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("endValue"), FixedString64Bytes.UTF8MaxLengthInBytes);
                }
                else if (valueType == typeof(FixedString128Bytes))
                {
                    relativeField =
                        PropertyFieldHelper.CreateFixedStringField(relative, FixedString128Bytes.UTF8MaxLengthInBytes);
                    startValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("startValue"), FixedString128Bytes.UTF8MaxLengthInBytes);
                    endValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("endValue"), FixedString128Bytes.UTF8MaxLengthInBytes);
                }
                else if (valueType == typeof(FixedString512Bytes))
                {
                    relativeField =
                        PropertyFieldHelper.CreateFixedStringField(relative, FixedString512Bytes.UTF8MaxLengthInBytes);
                    startValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("startValue"), FixedString512Bytes.UTF8MaxLengthInBytes);
                    endValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("endValue"), FixedString512Bytes.UTF8MaxLengthInBytes);
                }
                else if (valueType == typeof(FixedString4096Bytes))
                {
                    relativeField =
                        PropertyFieldHelper.CreateFixedStringField(relative, FixedString4096Bytes.UTF8MaxLengthInBytes);
                    startValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("startValue"), FixedString4096Bytes.UTF8MaxLengthInBytes);
                    endValueField = PropertyFieldHelper.CreateFixedStringField(
                        property.FindPropertyRelative("endValue"), FixedString4096Bytes.UTF8MaxLengthInBytes);
                }
                else
                {
                    relativeField = new PropertyField(relative);
                    startValueField = new PropertyField(property.FindPropertyRelative("startValue"));
                    endValueField = new PropertyField(property.FindPropertyRelative("endValue"));
                }

                group.Add(relativeField);
                group.Add(startValueField);
                group.Add(endValueField);

                void Refresh()
                {
                    startValueField.style.display =
                        relative.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
                }

                Refresh();

                group.TrackPropertyValue(relative, _ => Refresh());

                AddPropertyField(group, property, "duration");
            });

            Group(foldout, group =>
            {
                var ease = property.FindPropertyRelative("ease");
                group.Add(new PropertyField(ease));

                var customEaseCurve = new PropertyField(property.FindPropertyRelative("customEaseCurve"));
                customEaseCurve.style.display = ease.enumValueIndex == (int)Ease.CustomAnimationCurve ? DisplayStyle.Flex : DisplayStyle.None;

                group.Add(customEaseCurve);
                customEaseCurve.schedule
                    .Execute(() =>
                    {
                        try
                        {
                            customEaseCurve.style.display = ease.enumValueIndex == (int)Ease.CustomAnimationCurve ? DisplayStyle.Flex : DisplayStyle.None;
                        }
                        catch (InvalidOperationException)
                        {
                            // do nothing
                        }
                    })
                    .Every(10);
            });

            Group(foldout, group =>
            {
                AddPropertyField(group, property, "delay");
                AddPropertyField(group, property, "delayType");
            });

            Group(foldout, group =>
            {
                AddPropertyField(group, property, "loops");
                AddPropertyField(group, property, "loopType");
            });

            var options = property.FindPropertyRelative("options");
            if (options.hasChildren)
            {
                Group(foldout, group =>
                {
                    group.style.paddingLeft = 15f;
                    group.Add(new PropertyField(options));
                });
            }

            Group(foldout, group =>
            {
                FoldoutGroup(group, "Additional Settings", property.FindPropertyRelative("additionalSettings"), group =>
                {
                    group.style.marginLeft = 15f;
                    AddPropertyField(group, property, "cancelOnError");
                    AddPropertyField(group, property, "skipValuesDuringDelay");
                    AddPropertyField(group, property, "immediateBind");
                    group.Add(new PropertyField(property.FindPropertyRelative("schedulerType"), "Scheduler"));
                });
            });

            return foldout;
        }

        void Group(VisualElement root, Action<VisualElement> configure)
        {
            var group = new UnityEngine.UIElements.Box() { style = { marginBottom = 3f } };
            configure(group);
            root.Add(group);
        }

        void FoldoutGroup(VisualElement root, string label, SerializedProperty property, Action<VisualElement> configure)
        {
            var group = new Foldout
            {
                text = label,
                style = { marginBottom = 3f },
            };
            group.BindProperty(property);
            configure(group);
            root.Add(group);
        }

        void AddPropertyField(VisualElement root, SerializedProperty property, string name)
        {
            root.Add(new PropertyField(property.FindPropertyRelative(name)));
        }

        IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
        {
            var currentProperty = property.Copy();
            var nextProperty = property.Copy();
            nextProperty.Next(false);

            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextProperty)) break;
                    yield return currentProperty;
                }
                while (currentProperty.Next(false));
            }
        }
    }
}
