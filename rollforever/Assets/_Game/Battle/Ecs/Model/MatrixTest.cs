using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Ecs.Model
{
    public class MatrixTest : MonoBehaviour
    {
        [Header("Scan")]
        [SerializeField] private bool testScan;
        [SerializeField] private float2 prevScanPosition;
        [SerializeField] private float2 scanPosition;
        [SerializeField] private float scanRadius;
        [SerializeField] private TestVisitor result;
        [Header("Point")]
        [SerializeField] private bool testPoint;
        [SerializeField] List<Vector2Int> cellPoints = new List<Vector2Int>();

        private Vector2Int[] scanPoints = new Vector2Int[0];
        private Matrix matrix;

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if(testScan && matrix != null)
            {
                UnityEditor.Handles.DrawWireDisc((Vector2)prevScanPosition, Vector3.forward, scanRadius);
                UnityEditor.Handles.DrawWireDisc((Vector2)scanPosition, Vector3.forward, scanRadius);
                foreach (var item in scanPoints)
                {
                    Vector2 worldPosition = matrix.CellToWorld(new int2(item.x, item.y));
                    UnityEditor.Handles.DrawWireCube(worldPosition, Vector2.one * matrix.CellSize);
                }
            }

            if (testPoint && matrix != null)
            {
                Vector2Int[] array = cellPoints.ToArray();
                List<Vector3> worldPositions = new List<Vector3>();
                foreach (var p in array)
                {
                    Vector2 position = matrix.CellToWorld(new int2(p.x, p.y));
                    worldPositions.Add(position);
                    UnityEditor.Handles.DrawWireCube(position, Vector2.one * matrix.CellSize);
                }

                UnityEditor.Handles.DrawLines(worldPositions.ToArray());
            }
#endif
        }

        private void Start()
        {
            matrix = new Matrix(100, 120, 0.5f);
        }

        private void Update()
        {
            if(testScan)
            {
                scanPoints = result.points.ToArray();
                result.entities.Clear();
                result.points.Clear();
                matrix.ScanArea(prevScanPosition, scanPosition, scanRadius, result);
            }
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