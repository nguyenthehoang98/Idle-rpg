using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle
{
    public class BattleDebugView : MonoBehaviour
    {
        [SerializeField] private bool enableCellMatrix;
        [SerializeField] private List<CellMatrix> list = new List<CellMatrix>();
        private Matrix matrix;
        private FieldInfo cellsField;

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (enableCellMatrix)
            {
                CellMatrix[] copy = list.ToArray();
                foreach (var item in copy)
                {
                    Vector2 worldPosition = matrix.CellToWorld(item.point);
                    UnityEditor.Handles.Label(worldPosition, $"[{item.point.x}, {item.point.y}]");
                    UnityEditor.Handles.DrawWireCube(worldPosition, Vector2.one * matrix.CellSize);
                }
            }
#endif
        }

        public void InjectMatrix(Matrix matrix)
        {
            FieldInfo field = typeof(Matrix).GetField("cells",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError("Field 'cells' in Matrix not found");
                return;
            }
            
            cellsField = field;
            this.matrix = matrix;
        }

        private void LateUpdate()
        {
            if (cellsField != null && matrix != null)
            {
                list.Clear();
                Cell[,] cells = cellsField.GetValue(matrix) as Cell[,];
                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    for (int y = 0; y < cells.GetLength(1); y++)
                    {
                        var cell = cells[x, y];
                        if (cell.entities.IsCreated && cell.entities.Length > 0)
                        {
                            List<int> entities = new List<int>();
                            for (int i = 0; i < cell.entities.Length; i++)
                            {
                                entities.Add(cell.entities[i]);
                            }
                            
                            list.Add(new CellMatrix
                            {
                                trigger = cell.trigger, occupied = cell.occupied,
                                point = new int2(x, y), entities = string.Join(',', entities),
                            });
                        }
                    }
                }
            }
        }

        [Serializable]
        struct CellMatrix
        {
            public int2 point;
            public bool occupied;
            public bool trigger;
            public string entities;
        }
    }
}