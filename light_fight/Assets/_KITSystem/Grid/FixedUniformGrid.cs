using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace _KITSystem.Grid
{
    public class FixedUniformGrid : IGridManager
    {
        private readonly Dictionary<int, List<int>> cells = new();
        private readonly Dictionary<int, int> objectToCell = new();
        private readonly HashSet<int> visited = new();
        private readonly float cellSize;
        
        public FixedUniformGrid(float cellSize)
        {
            this.cellSize = cellSize;
        }
        
        public bool Insert(int unitId, Vector3 position)
        {
            int id = PositionToCell(position);

            // object đã tồn tại
            if (objectToCell.TryGetValue(unitId, out int oldCell))
            {
                if (oldCell == id)
                    return false;

                if (cells.TryGetValue(oldCell, out var oldList))
                {
                    oldList.Remove(unitId);

                    if (oldList.Count == 0)
                    {
                        cells.Remove(oldCell);
                    }
                }
            }

            // add vào cell mới
            if (!cells.TryGetValue(id, out var newList))
            {
                newList = new List<int>();

                cells[id] = newList;
            }

            newList.Add(unitId);

            objectToCell[unitId] = id;
            return true;
        }

        public bool Remove(int id)
        {
            if (!objectToCell.TryGetValue(id, out int cell))
                return false;

            if (cells.TryGetValue(cell, out var list))
            {
                list.Remove(id);

                if (list.Count == 0)
                {
                    cells.Remove(cell);
                }
            }

            return objectToCell.Remove(id);
        }

        public bool Query(Vector3 position, float radius, List<int> results)
        {
            visited.Clear();
            int range = Mathf.CeilToInt(radius / cellSize);
            int centerX = Mathf.FloorToInt(position.x / cellSize);
            int centerY = Mathf.FloorToInt(position.y / cellSize);

            for (int y = -range; y <= range; y++)
            {
                for (int x = -range; x <= range; x++)
                {
                    int hash = Hash(centerX + x, centerY + y);

                    if (!cells.TryGetValue(hash, out var list))
                        continue;

                    foreach (int id in list)
                    {
                        if (!visited.Add(id))
                            continue;

                        results.Add(id);
                    }
                }
            }

            return results.Count > 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PositionToCell(Vector2 position)
        {
            int x = Mathf.FloorToInt(position.x / cellSize);
            int y = Mathf.FloorToInt(position.y / cellSize);
            return Hash(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Hash(int x, int y) => x * 73856093 ^ y * 19349663;
    }
}