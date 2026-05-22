using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace _KITSystem.Grid
{
    [System.Serializable]
    public class FixedUniformGrid : IGridManager
    {
        private readonly float invCellSize;

        // Flat entry pool với linked list per cell
        [SerializeField] private int[] entryCellHash;
        [SerializeField] private int[] entryUnitId;
        [SerializeField] private int[] entryNext;
        [SerializeField] private int entryCount;
        private int freeHead = -1;

        // cellHash -> headEntryIndex
        private readonly Dictionary<int, int> cells = new(256);
        // unitId -> entryIndex
        private readonly Dictionary<int, int> unitToEntry = new(256);

        // Versioned visited thay cho HashSet (zero alloc)
        private int[] visited;
        private int visitedVersion;

        // Pre-allocated result buffer
        private int[] resultBuffer;

        public FixedUniformGrid(float cellSize, int initialCapacity = 256)
        {
            invCellSize = 1f / cellSize;

            entryCellHash = new int[initialCapacity];
            entryUnitId = new int[initialCapacity];
            entryNext = new int[initialCapacity];
            for (int i = 0; i < initialCapacity; i++) entryNext[i] = -1;

            visited = new int[initialCapacity];
            visitedVersion = 1;

            resultBuffer = new int[initialCapacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AllocEntry()
        {
            if (freeHead >= 0)
            {
                int idx = freeHead;
                freeHead = (entryNext[idx] < 0) ? -1 : entryNext[idx];
                entryNext[idx] = -1;
                return idx;
            }

            if (entryCount >= entryCellHash.Length)
            {
                int newSize = entryCellHash.Length * 2;
                System.Array.Resize(ref entryCellHash, newSize);
                System.Array.Resize(ref entryUnitId, newSize);
                System.Array.Resize(ref entryNext, newSize);
                for (int i = entryCount; i < newSize; i++) entryNext[i] = -1;
                System.Array.Resize(ref visited, newSize);
            }

            int idx2 = entryCount;
            entryNext[idx2] = -1;
            entryCount++;
            return idx2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FreeEntry(int idx)
        {
            entryNext[idx] = freeHead;
            freeHead = idx;
        }

        public bool Insert(int unitId, float2 position)
        {
            int cellHash = PositionToCell(position.x, position.y);
            if (unitToEntry.TryGetValue(unitId, out int oldEntry))
            {
                int oldHash = entryCellHash[oldEntry];
                if (oldHash == cellHash)
                    return false;

                RemoveFromCell(oldEntry, oldHash);
                entryCellHash[oldEntry] = cellHash;
                AddToCell(oldEntry, cellHash);
                return true;
            }

            int idx = AllocEntry();
            entryCellHash[idx] = cellHash;
            entryUnitId[idx] = unitId;
            AddToCell(idx, cellHash);
            unitToEntry[unitId] = idx;
            return true;
        }

        public bool Remove(int unitId)
        {
            if (!unitToEntry.TryGetValue(unitId, out int idx))
                return false;

            int hash = entryCellHash[idx];
            RemoveFromCell(idx, hash);
            unitToEntry.Remove(unitId);
            FreeEntry(idx);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToCell(int entryIdx, int cellHash)
        {
            if (cells.TryGetValue(cellHash, out int head))
            {
                entryNext[entryIdx] = head;
            }
            else
            {
                entryNext[entryIdx] = -1;
            }
            cells[cellHash] = entryIdx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveFromCell(int entryIdx, int cellHash)
        {
            if (!cells.TryGetValue(cellHash, out int head))
            {
                return;
            }

            if (head == entryIdx)
            {
                int next = entryNext[entryIdx];
                if (next >= 0)
                {
                    cells[cellHash] = next;
                }
                else
                {
                    cells.Remove(cellHash);
                }
                return;
            }

            int prev = head;
            int curr = entryNext[head];
            int steps = 0;
            while (curr >= 0 && steps < entryCount)
            {
                if (curr == entryIdx)
                {
                    entryNext[prev] = entryNext[curr];
                    return;
                }
                prev = curr;
                curr = entryNext[curr];
                steps++;
            }
        }

        public int Query(float2 position, float2 size, out int[] results)
        {
            int count = 0;
            visitedVersion++;

            int rangeX = (int)math.ceil(size.x * invCellSize);
            int rangeY = (int)math.ceil(size.y * invCellSize);
            int centerX = (int)math.floor(position.x * invCellSize);
            int centerY = (int)math.floor(position.y * invCellSize);

            for (int dy = -rangeY; dy <= rangeY; dy++)
            {
                for (int dx = -rangeX; dx <= rangeX; dx++)
                {
                    int hash = Hash(centerX + dx, centerY + dy);

                    if (!cells.TryGetValue(hash, out int head))
                        continue;

                    int curr = head;
                    int steps = 0;
                    while (curr >= 0 && steps < entryCount)
                    {
                        int unitId = entryUnitId[curr];
                        int next = entryNext[curr];
                        
                        if (unitId >= 0)
                        {
                            if (unitId >= visited.Length)
                            {
                                int newLen = math.max(visited.Length * 2, unitId + 1);
                                System.Array.Resize(ref visited, newLen);
                            }

                            if (visited[unitId] != visitedVersion)
                            {
                                visited[unitId] = visitedVersion;

                                if (count >= resultBuffer.Length)
                                {
                                    System.Array.Resize(ref resultBuffer, resultBuffer.Length * 2);
                                }

                                resultBuffer[count++] = unitId;
                            }
                        }

                        curr = next;
                        steps++;
                    }
                }
            }

            results = resultBuffer;
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PositionToCell(float x, float y)
        {
            int cx = (int)math.floor(x * invCellSize);
            int cy = (int)math.floor(y * invCellSize);
            return Hash(cx, cy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Hash(int x, int y) => (x * 73856093) ^ (y * 19349663);
    }
}
