#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _KITSystem.Schedule.Editor
{
    [CustomEditor(typeof(TickSystemOwner))]
    public class TickSystemOwnerEditor : UnityEditor.Editor
    {
        private SerializedProperty tickables;

        private Type[] tickableTypes;

        private void OnEnable()
        {
            tickables = serializedObject.FindProperty("tickables");

            tickableTypes = TypeCache.GetTypesDerivedFrom<ITickable>()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name)
                .ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "tickables");

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Tickables", EditorStyles.boldLabel);

            for (int i = 0; i < tickables.arraySize; i++)
            {
                DrawElement(i);
            }

            if (GUILayout.Button("Add Tickable"))
            {
                ShowAddMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawElement(int index)
        {
            var element = tickables.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            string typeName = element.managedReferenceValue == null
                ? "Null"
                : element.managedReferenceValue.GetType().Name;

            EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                tickables.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(element, GUIContent.none, true);

            EditorGUILayout.EndVertical();
        }

        private void ShowAddMenu()
        {
            var menu = new GenericMenu();

            foreach (var type in tickableTypes)
            {
                menu.AddItem(
                    new GUIContent(type.Name),
                    false,
                    () => AddTickable(type));
            }

            menu.ShowAsContext();
        }

        private void AddTickable(Type type)
        {
            serializedObject.Update();

            tickables.arraySize++;

            SerializedProperty element = tickables.GetArrayElementAtIndex(tickables.arraySize - 1);
            element.managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
#endif