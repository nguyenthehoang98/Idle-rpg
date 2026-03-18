using System;
using System.Collections.Generic;
using _Games.Combat.SkillSystem.Config;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CircleColliderSO))]
public class CircleColliderSOEditor : Editor
{
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnGUIInternal;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnGUIInternal;
    }

    private void OnGUIInternal(SceneView obj)
    {
        CircleColliderSO data = (CircleColliderSO)target;
        DrawGizmos(data.list, Vector3.zero);
    }

    private void DrawGizmos(List<CircleColliderSO.Circle> list, Vector3 center)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            CircleColliderSO.Circle circle = list[i];
            float radius = circle.radius;
            Handles.color = Color.green;
            Handles.DrawWireArc(center + circle.offset, Vector3.forward, Vector3.right, 360, radius);
            
            if (circle.adjustRadius)
            {
                Handles.color = Color.red;
                Handles.DrawWireArc(center + circle.offset, Vector3.forward, Vector3.right, 360, circle.extraRadius);
            }
        }
    }
}