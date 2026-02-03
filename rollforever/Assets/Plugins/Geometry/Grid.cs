using System;
using System.Collections.Generic;
using _KIT.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public sealed class Grid<T> : IDisposable where T : IShapeData
    {
        readonly float cellSize;
        readonly float invCellSize;
        
        readonly Dictionary<int, CellBound> objectCells;

        // cell -> list of collider id
        readonly Dictionary<long, List<int>> grid;

        // id -> collider
        readonly Dictionary<int, T> colliders;
        readonly List<T> list;

        // snapshot for query (reuse)
        readonly HashSet<int> visited;

        public Grid(float2 worldSize, float cellSize, int capacity)
        {
            invCellSize = 1f / cellSize;
            var cellCapacity =
                (int)(worldSize.x / cellSize) *
                (int)(worldSize.y / cellSize);

            objectCells = new Dictionary<int, CellBound>(capacity);
            grid = new Dictionary<long, List<int>>(cellCapacity);
            colliders = new Dictionary<int, T>(capacity);
            list = new List<T>();
            visited = new HashSet<int>(capacity);
        }

        // ================================
        // Cell Key
        // ================================
        private static long CellKey(int x, int y)
        {
            return ((long)x << 32) | (uint)y;
        }

        // ================================
        // Insert / Update
        // ================================
        public void InsertOrUpdate(T data)
        {
            var id = data.Id;

            if (colliders.TryGetValue(id, out var old))
            {
                RemoveFromCells(old);
            }
            else
            {
                list.Add(data);
            }

            var bound = CalculateCellBound(data.Position, data.HalfSizeBound, invCellSize);

            objectCells[id] = bound;
            colliders[id] = data;
            AddToCells(id, bound);
        }

        public void Remove(int id)
        {
            if (!colliders.TryGetValue(id, out var data))
                return;

            RemoveFromCells(data);
            colliders.Remove(id);
            CollectionUtils.RemoveFast(list, data);
        }

        public void Remove(T data)
        {
            Remove(data.Id);
        }

        // ================================
        // Internal add / remove
        // ================================
        private void AddToCells(int id, CellBound b)
        {
            for (var x = b.xmin; x <= b.xmax; x++)
            for (var y = b.ymin; y <= b.ymax; y++)
            {
                var key = CellKey(x, y);

                if (!grid.TryGetValue(key, out var list))
                {
                    list = new List<int>(4);
                    grid[key] = list;
                }

                list.Add(id);
            }
        }

        private void RemoveFromCells(T data)
        {
            var id = data.Id;
            if (!objectCells.TryGetValue(id, out var b))
                return;

            for (var x = b.xmin; x <= b.xmax; x++)
            for (var y = b.ymin; y <= b.ymax; y++)
            {
                var key = CellKey(x, y);

                if (grid.TryGetValue(key, out var list))
                {
                    CollectionUtils.RemoveFast(list, data.Id);
                }
            }
        }

        // ================================
        // Core Query (Broadphase ONLY)
        // ================================
        public int Query(CellBound bound, ref List<T> results)
        {
            visited.Clear();
            results.Clear();

            for (var x = bound.xmin; x <= bound.xmax; x++)
            for (var y = bound.ymin; y <= bound.ymax; y++)
            {
                var key = CellKey(x, y);

                if (!grid.TryGetValue(key, out var list))
                    continue;

                for (var i = 0; i < list.Count; i++)
                {
                    var id = list[i];
                    if (!visited.Add(id))
                        continue;

                    if (colliders.TryGetValue(id, out var data))
                    {
                        results.Add(data);
                    }
                }
            }

            return results.Count;
        }

        public bool QueryAny(CellBound bound)
        {
            visited.Clear();

            for (var x = bound.xmin; x <= bound.xmax; x++)
            for (var y = bound.ymin; y <= bound.ymax; y++)
            {
                var key = CellKey(x, y);

                if (!grid.TryGetValue(key, out var list))
                    continue;

                if (list.Count > 0) return true;
            }

            return false;
        }
        
        // ================================
        // Tìm unit gần nhất
        // ================================
        public bool FindObjectClosestSnapshot(float2 closest, out T data)
        {
            data = default;
            var found = false;
            
            if (list.Count > 0)
            {
                var min = float.MaxValue;
                var count = list.Count;
                for (var i = 0; i < count; i++)
                {
                    var d = math.distancesq(closest, list[i].Position);
                    if (d < min)
                    {
                        min = d;
                        data = list[i];
                        found = true;
                    }
                }
            }

            return found;
        }

        public bool OverlapAnyObject(float2 closest, float2 size)
        {
            return QueryAny(CalculateCellBound(closest, size / 2, invCellSize));
        }
        
        public void Draw(Color color, float duration)
        {
#if UNITY_EDITOR
            foreach (var kv in colliders)
            {
                var b = objectCells[kv.Key];

                var xmin = b.xmin * cellSize;
                var xmax = (b.xmax + 1) * cellSize;
                var ymin = b.ymin * cellSize;
                var ymax = (b.ymax + 1) * cellSize;

                var bl = new Vector3(xmin, ymin);
                var br = new Vector3(xmax, ymin);
                var tr = new Vector3(xmax, ymax);
                var tl = new Vector3(xmin, ymax);
                
                UnityEditor.Handles.DrawPolyLine(new Vector3[]
                {
                    bl, br, tr, tl
                });
                
                Debug.Log($"{bl}, {br}, {tr}, {tl}");
            }
#endif
        }

        public void Dispose()
        {
        }
        
        static CellBound CalculateCellBound(Vector2 position, Vector2 halfSize, float invCellSize)
        {
            var minX = Mathf.FloorToInt((position.x - halfSize.x) * invCellSize);
            var maxX = Mathf.FloorToInt((position.x + halfSize.x) * invCellSize);
            var minY = Mathf.FloorToInt((position.y - halfSize.y) * invCellSize);
            var maxY = Mathf.FloorToInt((position.y + halfSize.y) * invCellSize);

            return new CellBound(minX, maxX, minY, maxY);
        }
    }
    
    public readonly struct CellBound
    {
        public readonly int xmin;
        public readonly int xmax;
        public readonly int ymin;
        public readonly int ymax;

        public CellBound(int xmin, int xmax, int ymin, int ymax)
        {
            this.xmin = xmin;
            this.xmax = xmax;
            this.ymin = ymin;
            this.ymax = ymax;
        }

        public bool Compare(CellBound bound)
        {
            return xmin == bound.xmin && xmax == bound.xmax && ymin == bound.ymin && ymax == bound.ymax;
        }

        public override string ToString()
        {
            return $"x=[{xmin}:{xmax}], y=[{ymin}:{ymax}]";
        }
    }
}
