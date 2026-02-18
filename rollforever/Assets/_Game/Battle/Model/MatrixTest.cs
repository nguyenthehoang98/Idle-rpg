using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle
{
    public class MatrixTest : MonoBehaviour
    {
        [SerializeField] private float2 scanPosition;
        [SerializeField] private float scanRadius;
        [SerializeField] private TestVisitor result;

        private Vector2Int[] points = new Vector2Int[0];
        private Matrix matrix;

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.DrawWireDisc((Vector2)scanPosition, Vector3.forward, scanRadius);
            foreach (var item in points)
            {
                Vector2 worldPosition = matrix.CellToWorld(new int2(item.x, item.y));
                //UnityEditor.Handles.Label(worldPosition, $"[{item.x}, {item.y}]");
                UnityEditor.Handles.DrawWireCube(worldPosition, Vector2.one * matrix.CellSize);
            }
#endif
        }

        private void Start()
        {
            matrix = new Matrix(100, 120, 0.5f);
        }

        private void Update()
        {
            points = result.points.ToArray();
            
            result.entities.Clear();
            result.points.Clear();

            
            matrix.ScanArea(scanPosition, scanRadius, result);
        }

        [Serializable]
        class TestVisitor : IVisitor
        {
            public List<int> entities = new List<int>();
            public List<Vector2Int> points = new List<Vector2Int>();
            
            public void Visit(int entity)
            {
                entities.Add(entity);
            }

            public void VisitCell(int x, int y)
            {
                points.Add(new Vector2Int(x, y));
            }
        }
    }
}