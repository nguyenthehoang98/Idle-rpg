using System;
using System.Collections.Generic;
using System.Reflection;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle
{
    public class BattleDebugView : MonoBehaviour, IEcsWorldEventListener
    {
        [SerializeField] private bool enableCellMatrix;
        [SerializeField] private List<CellMatrix> list = new List<CellMatrix>();

        private BattleStartupShareData shareData;
        private Matrix matrix;
        private FieldInfo cellsField;

        private int totalEntityCreated;
        private int totalEntityDestroyed;

        private const float referenceHeight = 1080f;
        private GUIStyle style;
        private float cachedScale = -1f;
        
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

        public void InjectShareData(BattleStartupShareData shareData)
        {
            FieldInfo field = typeof(Matrix).GetField("cells",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError("Field 'cells' in Matrix not found");
                return;
            }

            this.cellsField = field;
            this.matrix = shareData.Matrix;
            this.shareData = shareData;
        }

        void OnGUI()
        {
            float scale = Screen.height / referenceHeight;

            // Chỉ rebuild style khi scale thay đổi (tránh GC)
            if (style == null || Mathf.Abs(scale - cachedScale) > 0.01f)
            {
                cachedScale = scale;

                style = new GUIStyle(GUI.skin.label);
                style.fontSize = Mathf.RoundToInt(40 * scale);
                style.alignment = TextAnchor.UpperLeft;
                style.normal.textColor = Color.white;
            }

            Rect rect1 = new Rect(20 * scale, 20 * scale, 500 * scale, 100 * scale);
            Rect rect2 = new Rect(20 * scale, 120 * scale, 500 * scale, 100 * scale);
            Rect rect3 = new Rect(20 * scale, 220 * scale, 500 * scale, 100 * scale);
            GUI.Label(rect1, $"Total Destroyed: {totalEntityDestroyed}", style);

            int time = 0;
            if (shareData != null) time = (int)shareData.Time;
            GUI.Label(rect2, $"Total Time: {time}", style);
            GUI.Label(rect3, $"Engine Time: {(int)(Time.time)}", style);
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

        public void OnEntityCreated(int entity)
        {
            totalEntityCreated++;
        }

        public void OnEntityChanged(int entity, short poolId, bool added)
        {

        }

        public void OnEntityDestroyed(int entity)
        {
            totalEntityDestroyed++;
        }

        public void OnFilterCreated(EcsFilter filter)
        {

        }

        public void OnWorldResized(int newSize)
        {

        }

        public void OnWorldDestroyed(EcsWorld world)
        {

        }
    }
}