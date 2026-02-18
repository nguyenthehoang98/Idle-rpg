using _Game.Battle.Data;
using Geometry.Primary;
using Leopotam.EcsLite.UnityEditor;
using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Editor.Ecs
{
    class ShapeDataInspector : EcsComponentInspectorTyped<ShapeData>
    {
        public override bool OnGuiTyped(string label, ref ShapeData value, EcsEntityDebugView entityView)
        {
            var type = value.Value.type;
            EditorGUILayout.EnumPopup("Type", type);
            switch (type)
            {
                case ShapeType.Box:
                    EditorGUILayout.Vector2Field("Size", value.Value.size);
                    break;
                case ShapeType.Circle:
                    EditorGUILayout.FloatField("Radius", value.Value.radius);
                    break;
                default:
                    Debug.LogError("Shape inspector type not supported: " + type);
                    break;
            }

            return false;
        }
    }
}