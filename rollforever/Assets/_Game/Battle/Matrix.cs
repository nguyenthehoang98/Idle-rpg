using Unity.Collections;
using Unity.Mathematics;

namespace _Game.Battle
{
    public sealed class Matrix
    {
        private float cellSize;
        private int width;
        private int height;
        private Cell[,] matrix;

        public Matrix(int width, int height, float cellSize)
        {
            this.cellSize = cellSize;
            this.width = width;
            this.height = height;
            this.matrix = new Cell[width, height];
        }

        public void ResetTrigger()
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                    matrix[i, j].trigger = false;
            }
        }
        
        public void TriggerPoint(float2 position)
        {
            var cell = WorldToCell(position);
            int x = cell.x;
            int y = cell.y;
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                matrix[x, y].trigger = true;
            }
        }

        public void OccupiedPoint(float2 position)
        {
            var cell = WorldToCell(position);
            int x = cell.x;
            int y = cell.y;
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                matrix[x, y].occupied = true;
            }
        }
        
        public void TriggerArea(float2 position, float radius)
        {
            var centerCell = WorldToCell(position);
            int rangeX = (int)(radius / cellSize);
            int rangeY = (int)(radius / cellSize);

            for (int dx = -rangeX; dx <= rangeX; dx++)
            {
                for (int dy = -rangeY; dy <= rangeY; dy++)
                {
                    int x = centerCell.x + dx;
                    int y = centerCell.y + dy;

                    if (x < 0 || y < 0 || x >= width || y >= height)
                        continue;

                    float2 cellWorldPos = CellToWorldCenter(x, y);
                    if (math.distancesq(cellWorldPos, position) <= radius * radius)
                    {
                        matrix[x, y].trigger = true;
                    }
                }
            }
        }

        public bool TryFindCellExpandFromCenter(float2 position, float2 pivot, out float2 result)
        {
            int2 center = WorldToCell(pivot);
            int2 snapshot = new int2(-1, -1);

            var bestDistToPoint = float.MaxValue;
            var maxDist = math.max(width, height);

            for (var dist = 0; dist <= maxDist; dist++)
            {
                var foundAtThisDist = false;
                bestDistToPoint = float.MaxValue;

                for (var dx = -dist; dx <= dist; dx++)
                for (var dy = -dist; dy <= dist; dy++)
                {
                    if (math.max(math.abs(dx), math.abs(dy)) != dist)
                        continue;

                    var c = new int2(center.x + dx, center.y + dy);

                    if (!IsInsideGrid(c))
                        continue;

                    if (!IsEmpty(c))
                        continue;

                    if (!HasAnyFreeNeighbor8(c))
                        continue;

                    var wp = CellToWorld(c);
                    var dToPoint = math.lengthsq(wp - position);

                    if (!foundAtThisDist || dToPoint < bestDistToPoint)
                    {
                        foundAtThisDist = true;
                        bestDistToPoint = dToPoint;
                        snapshot = c;
                    }
                }

                if (foundAtThisDist)
                {
                    result = CellToWorld(snapshot);
                    return true;
                }
            }

            result = CellToWorld(snapshot);
            return false;
        }

        public float radiussq(float radius) => (radius + cellSize) * (radius + cellSize) + float.Epsilon;
        
        int2 WorldToCell(float2 worldPos) => WorldToCell(worldPos, float2.zero);

        int2 WorldToCell(float2 worldPos, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            var x = (int) math.floor(
                (worldPos.x - (gridCenter.x - halfGrid.x)) / cellSize
            );
            var y = (int) math.floor(
                (worldPos.y - (gridCenter.y - halfGrid.y)) / cellSize
            );
            return new int2(x, y);
        }
        
        float2 HalfGridSize() => new float2(width * cellSize * 0.5f, height * cellSize * 0.5f);
        
        float2 CellToWorldCenter(int x, int y) => CellToWorldCenter(x, y, float2.zero);
        
        float2 CellToWorldCenter(int x, int y, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            return new float2(
                gridCenter.x - halfGrid.x + (x + 0.5f) * cellSize,
                gridCenter.y - halfGrid.y + (y + 0.5f) * cellSize
            );
        }
        
        bool IsInsideGrid(int2 c) => c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;

        bool IsEmpty(int2 c)
        {
            var data = matrix[c.x, c.y];
            return IsInsideGrid(c) && !data.occupied && !data.trigger;
        }

        float2 CellToWorld(int2 cell) => CellToWorld(cell, float2.zero);

        float2 CellToWorld(int2 cell, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            return new float2(
                gridCenter.x - halfGrid.x + (cell.x + 0.5f) * cellSize,
                gridCenter.y - halfGrid.y + (cell.y + 0.5f) * cellSize
            );
        }
        
        bool HasAnyFreeNeighbor8(int2 cell)
        {
            for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                var n = new int2(cell.x + dx, cell.y + dy);

                if (!IsInsideGrid(n))
                    continue;

                if (IsEmpty(n)) return true;
            }

            return false; // bị bao vây hoàn toàn
        }
    }

    public struct Cell
    {
        public bool occupied;
        public bool trigger;
        public NativeList<int> agentList;
    }
}