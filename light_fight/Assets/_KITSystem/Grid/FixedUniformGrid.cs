using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace _KITSystem.Grid
{
    public class FixedUniformGrid : IGridManager
    {
        //========================================================
        // GRID
        //========================================================

        private readonly List<int>[] cells;
        private readonly int[] objectToCell;
        private readonly int[] marks;
        private readonly float cellSize;
        private readonly int width;
        private readonly int height;

        private List<int> temp = new List<int>(16);
        private int currentMark;

        public FixedUniformGrid(
            int width,
            int height,
            float cellSize,
            int maxUnits)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;

            cells = new List<int>[width * height];

            objectToCell = new int[maxUnits];

            marks = new int[maxUnits];

            for (int i = 0; i < maxUnits; i++)
            {
                objectToCell[i] = -1;
            }
        }
        
        public bool Insert(int unitId, Vector3 position)
        {
            int cell = PositionToCell(position);

            if (cell < 0)
                return false;

            int oldCell = objectToCell[unitId];

            // same cell
            if (oldCell == cell)
                return false;

            // remove old
            if (oldCell >= 0)
            {
                List<int> oldList = cells[oldCell];

                if (oldList != null)
                {
                    int index = oldList.IndexOf(unitId);

                    if (index >= 0)
                    {
                        int last = oldList.Count - 1;

                        oldList[index] = oldList[last];

                        oldList.RemoveAt(last);
                    }
                }
            }

            // add new
            List<int> list = cells[cell];

            if (list == null)
            {
                list = new List<int>(8);

                cells[cell] = list;
            }

            list.Add(unitId);

            objectToCell[unitId] = cell;

            return true;
        }

        public bool Remove(int unitId)
        {
            int cell = objectToCell[unitId];

            if (cell < 0)
                return false;

            List<int> list = cells[cell];

            if (list != null)
            {
                int index = list.IndexOf(unitId);

                if (index >= 0)
                {
                    int last = list.Count - 1;

                    list[index] = list[last];

                    list.RemoveAt(last);
                }
            }

            objectToCell[unitId] = -1;

            return true;
        }


        public int Query(Vector3 position, float radius, out List<int> results)
        {
            int total = 0;

            currentMark++;

            int range =
                Mathf.CeilToInt(radius / cellSize);

            int centerX =
                Mathf.FloorToInt(position.x / cellSize);

            int centerY =
                Mathf.FloorToInt(position.y / cellSize);

            int minX = Mathf.Max(0, centerX - range);
            int maxX = Mathf.Min(width - 1, centerX + range);

            int minY = Mathf.Max(0, centerY - range);
            int maxY = Mathf.Min(height - 1, centerY + range);

            for (int y = minY; y <= maxY; y++)
            {
                int row = y * width;

                for (int x = minX; x <= maxX; x++)
                {
                    List<int> list = cells[row + x];

                    if (list == null)
                        continue;

                    int count = list.Count;

                    for (int i = 0; i < count; i++)
                    {
                        int id = list[i];

                        if (marks[id] == currentMark)
                            continue;

                        marks[id] = currentMark;

                        if (total < temp.Count)
                        {
                            temp[total] = id;
                        }
                        else
                        {
                            temp.Add(id);
                        }

                        total++;
                    }
                }
            }

            results = temp;
            return total;
        }

        //========================================================
        // HELPERS
        //========================================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PositionToCell(Vector2 position)
        {
            int x =
                Mathf.FloorToInt(position.x / cellSize);

            int y =
                Mathf.FloorToInt(position.y / cellSize);

            if ((uint)x >= width ||
                (uint)y >= height)
            {
                return -1;
            }

            return y * width + x;
        }
    }
}