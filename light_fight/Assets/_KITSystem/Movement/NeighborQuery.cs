using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Movement
{
    internal class NeighborQuery
    {
        private float cellSize;

        // dùng int hash thay vì Vector2Int
        private Dictionary<int, List<int>> grid = new(256);

        // cache neighbors để tránh new mỗi frame
        private List<int>[] neighborsCache;

        public NeighborQuery(float cellSize)
        {
            this.cellSize = cellSize;
        }

        public void BuildGrid(List<Vector3> positions)
        {
            // clear nhưng giữ capacity
            foreach (var kv in grid)
            {
                kv.Value.Clear();
            }

            // không clear dictionary để reuse bucket
            // grid.Clear(); ❌ tránh nếu muốn reuse tốt hơn

            for (int i = 0; i < positions.Count; i++)
            {
                GetCell(positions[i], out int x, out int y);
                int hash = Hash(x, y);

                if (!grid.TryGetValue(hash, out var list))
                {
                    list = new List<int>(8); // preset capacity nhỏ
                    grid[hash] = list;
                }

                list.Add(i);
            }
        }

        public List<int>[] QueryNeighbors(List<Vector3> positions, float radius)
        {
            int count = positions.Count;

            // init cache nếu cần
            if (neighborsCache == null || neighborsCache.Length != count)
            {
                neighborsCache = new List<int>[count];
                for (int i = 0; i < count; i++)
                {
                    neighborsCache[i] = new List<int>(8);
                }
            }

            float radiusSq = radius * radius;

            for (int i = 0; i < count; i++)
            {
                var neighbors = neighborsCache[i];
                neighbors.Clear();

                Vector3 posI = positions[i];

                GetCell(posI, out int cx, out int cy);

                // check 9 cells
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = cx + dx;

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int ny = cy + dy;
                        int hash = Hash(nx, ny);

                        if (!grid.TryGetValue(hash, out var list)) continue;

                        // loop tuyến tính (cache-friendly hơn một chút)
                        for (int k = 0; k < list.Count; k++)
                        {
                            int j = list[k];
                            if (j == i) continue;

                            Vector3 diff = posI - positions[j];

                            if (diff.sqrMagnitude <= radiusSq)
                            {
                                neighbors.Add(j);
                            }
                        }
                    }
                }
            }

            return neighborsCache;
        }

        private void GetCell(Vector3 pos, out int x, out int y)
        {
            x = (int)(pos.x / cellSize);
            y = (int)(pos.z / cellSize);
        }

        private int Hash(int x, int y)
        {
            return (x * 73856093) ^ (y * 19349663);
        }
    }
}