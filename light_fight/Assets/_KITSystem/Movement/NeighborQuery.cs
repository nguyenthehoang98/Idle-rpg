using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Movement
{
    internal class NeighborQuery
    {
        private float cellSize = 2.0f;

        private Dictionary<Vector2Int, List<int>> grid = new();

        public void BuildGrid(List<Vector3> positions)
        {
            grid.Clear();

            for (int i = 0; i < positions.Count; i++)
            {
                Vector2Int cell = GetCell(positions[i]);

                if (!grid.TryGetValue(cell, out var list))
                {
                    list = new List<int>();
                    grid[cell] = list;
                }

                list.Add(i);
            }
        }

        public List<int>[] QueryNeighbors(
            List<Vector3> positions,
            float radius)
        {
            List<int>[] neighbors = new List<int>[positions.Count];

            float radiusSq = radius * radius;

            for (int i = 0; i < positions.Count; i++)
            {
                if (neighbors[i] == null)
                    neighbors[i] = new List<int>();
                else
                    neighbors[i].Clear();

                Vector2Int center = GetCell(positions[i]);

                // check 9 cells
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int cell = new Vector2Int(center.x + x, center.y + y);

                        if (!grid.TryGetValue(cell, out var list)) continue;

                        for (int k = 0; k < list.Count; k++)
                        {
                            int j = list[k];
                            if (j == i) continue;

                            Vector3 diff = positions[i] - positions[j];
                            if (diff.sqrMagnitude <= radiusSq)
                            {
                                neighbors[i].Add(j);
                            }
                        }
                    }
                }
            }

            return neighbors;
        }

        private Vector2Int GetCell(Vector3 pos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(pos.x / cellSize),
                Mathf.FloorToInt(pos.z / cellSize) // dùng XZ plane
            );
        }
    }
}