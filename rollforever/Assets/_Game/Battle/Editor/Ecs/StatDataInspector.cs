using System.Reflection;
using _Game.Battle.Data;
using _Game.Battle.Model;
using Leopotam.EcsLite.UnityEditor;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Editor.Ecs
{
    class StatDataInspector : EcsComponentInspectorTyped<StatData>
    {
        public override bool OnGuiTyped(string label, ref StatData value, EcsEntityDebugView entityView)
        {
            var field = value.GetType().GetField("map",
                BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (field == null)
            {
                Debug.LogWarning("Field 'map' in StatData not found");
                return false;
            }

            if (field.GetValue(value) is NativeHashMap<int, Stat> map)
            {
                foreach (var pair in map)
                {
                    EditorGUILayout.FloatField(((StatType)pair.Key).ToString(), pair.Value.Value);   
                }
            }

            return false;
        }
    }
}