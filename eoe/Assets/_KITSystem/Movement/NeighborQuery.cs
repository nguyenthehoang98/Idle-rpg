using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace _KITSystem.Movement
{
    internal class NeighborQuery
    {
        private float cellSize;
        private float invCellSize;

        private readonly Dictionary<int, List<int>> grid = new(256);

        private List<int>[] neighborsCache;
        private int cachedCount;

        public NeighborQuery(float cellSize)
        {
            this.cellSize = cellSize;
            invCellSize = 1f / cellSize;
        }

        public void BuildGrid(NativeArray<float2> positions)
        {
            foreach (var kv in grid)
            {
                kv.Value.Clear();
            }

            for (int i = 0; i < positions.Length; i++)
            {
                int hash = PositionToCell(positions[i]);

                if (!grid.TryGetValue(hash, out var list))
                {
                    list = new List<int>(8);
                    grid[hash] = list;
                }

                list.Add(i);
            }
        }

        public List<int>[] QueryNeighbors(NativeArray<float2> positions, float radius)
        {
            int count = positions.Length;
            if (neighborsCache == null || neighborsCache.Length < count)
            {
                int oldLen = neighborsCache?.Length ?? 0;
                int newLen = math.max(count, oldLen * 2);
                var newArr = new List<int>[newLen];

                if (neighborsCache != null)
                {
                    System.Array.Copy(neighborsCache, newArr, oldLen);
                }

                for (int i = oldLen; i < newLen; i++)
                {
                    newArr[i] = new List<int>(8);
                }

                neighborsCache = newArr;
            }

            float radiusSq = radius * radius;
            int range = (int)math.ceil(radius * invCellSize);

            for (int i = 0; i < count; i++)
            {
                var neighbors = neighborsCache[i];
                neighbors.Clear();

                float2 posI = positions[i];
                int cx = (int)math.floor(posI.x * invCellSize);
                int cy = (int)math.floor(posI.y * invCellSize);

                for (int dy = -range; dy <= range; dy++)
                {
                    for (int dx = -range; dx <= range; dx++)
                    {
                        int hash = Hash(cx + dx, cy + dy);

                        if (!grid.TryGetValue(hash, out var list)) continue;

                        for (int k = 0; k < list.Count; k++)
                        {
                            int j = list[k];
                            if (j == i) continue;

                            float2 diff = posI - positions[j];
                            if (math.lengthsq(diff) <= radiusSq)
                            {
                                neighbors.Add(j);
                            }
                        }
                    }
                }
            }

            cachedCount = count;
            return neighborsCache;
        }

        public List<int> GetCachedNeighbors(int index)
        {
            if (index >= 0 && index < cachedCount && neighborsCache != null)
                return neighborsCache[index];
            return null;
        }

        private int PositionToCell(float2 pos)
        {
            int x = (int)math.floor(pos.x * invCellSize);
            int y = (int)math.floor(pos.y * invCellSize);
            return Hash(x, y);
        }

        private static int Hash(int x, int y) => (x * 73856093) ^ (y * 19349663);
    }
}
