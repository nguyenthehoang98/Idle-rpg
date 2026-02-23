using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Model
{
    public sealed class Matrix : IDisposable
    {
        private const int EXPECTED_MAX = 3;
        private float cellSize;
        private int width;
        private int height;
        private Cell[,] cells;

        public Matrix(int width, int height, float cellSize)
        {
            this.cellSize = cellSize;
            this.width = width;
            this.height = height;
            this.cells = new Cell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new Cell();
                }
            }
        }

        public void ResetTrigger()
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    cells[i, j].trigger = false;
                    if (cells[i, j].entities.IsCreated)
                        cells[i, j].entities.Clear();
                }
            }
        }

        public void TriggerPoint(int unit, float2 position)
        {
            var cell = WorldToCell(position);
            int x = cell.x;
            int y = cell.y;
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                cells[x, y].trigger = true;
                if (!cells[x, y].entities.IsCreated)
                    cells[x, y].entities = new NativeList<int>(EXPECTED_MAX, Allocator.Persistent);
                cells[x, y].entities.Add(unit);
            }
        }

        public void OccupiedPoint(int unit, float2 position)
        {
            var cell = WorldToCell(position);
            int x = cell.x;
            int y = cell.y;
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                cells[x, y].occupied = true;
                if (!cells[x, y].entities.IsCreated)
                    cells[x, y].entities = new NativeList<int>(EXPECTED_MAX, Allocator.Persistent);
                cells[x, y].entities.Add(unit);
            }
        }

        public void TriggerArea(int unit, float2 position, float radius)
        {
            var centerCell = WorldToCell(position);
            int rangeX = (int)(radius / cellSize);
            int rangeY = (int)(radius / cellSize);
            float rsq = radius * radius;
            for (int dx = -rangeX; dx <= rangeX; dx++)
            {
                for (int dy = -rangeY; dy <= rangeY; dy++)
                {
                    int x = centerCell.x + dx;
                    int y = centerCell.y + dy;

                    if (x < 0 || y < 0 || x >= width || y >= height)
                        continue;

                    float2 cellWorldPos = CellToWorldCenter(x, y);
                    if (math.distancesq(cellWorldPos, position) <= rsq)
                    {
                        cells[x, y].trigger = true;
                        if (!cells[x, y].entities.IsCreated)
                            cells[x, y].entities = new NativeList<int>(EXPECTED_MAX, Allocator.Persistent);
                        cells[x, y].entities.Add(unit);
                    }
                }
            }
        }

        public void TriggerArea(int unit, float2 position, float2 size)
        {
            int2 centerCell = WorldToCell(position);
            float2 halfSize = size * 0.5f;
            int rangeX = (int)(size.x / cellSize);
            int rangeY = (int)(size.y / cellSize);

            for (int dx = -rangeX; dx <= rangeX; dx++)
            {
                for (int dy = -rangeY; dy <= rangeY; dy++)
                {
                    int x = centerCell.x + dx;
                    int y = centerCell.y + dy;

                    if (x < 0 || y < 0 || x >= width || y >= height)
                        continue;

                    float2 cellWorldPos = CellToWorldCenter(x, y);
                    if (math.abs(cellWorldPos.x - position.x) <= halfSize.x &&
                        math.abs(cellWorldPos.y - position.y) <= halfSize.y)
                    {
                        cells[x, y].trigger = true;
                        if (!cells[x, y].entities.IsCreated)
                            cells[x, y].entities = new NativeList<int>(EXPECTED_MAX, Allocator.Persistent);
                        cells[x, y].entities.Add(unit);
                    }
                }
            }
        }

        public void ScanArea(Vector2 a, Vector2 b, float radius, IVisitor visitor)
        {
            float radiusSq = radius * radius;

            Vector2 dir = b - a;
            float lengthSq = dir.sqrMagnitude;

            Vector2 min = Vector2.Min(a, b) - Vector2.one * radius;
            Vector2 max = Vector2.Max(a, b) + Vector2.one * radius;

            int2 minCell = WorldToCell(min);
            int2 maxCell = WorldToCell(max);

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    int2 cell = new int2(x, y);
                    if (!IsInsideGrid(cell))
                        continue;

                    Vector2 p = CellToWorld(cell);

                    float t = math.dot(p - a, dir) / lengthSq;
                    t = math.clamp(t, 0f, 1f);

                    Vector2 closest = a + dir * t;

                    if (math.distancesq(p, closest) <= radiusSq)
                    {
                        visitor.VisitCell(x, y);

                        var list = cells[x, y].entities;
                        if (!list.IsCreated)
                            continue;

                        for (int i = 0; i < list.Length; i++)
                            visitor.Visit(list[i]);
                    }
                }
            }
        }
        
        public void ScanArea(Vector2 a, Vector2 b, Vector2 size, IVisitor visitor)
        {
            Vector2 delta = b - a;
            float length = delta.magnitude;

            if (length < 0.0001f)
            {
                // fallback thành scan circle nhỏ
                ScanArea(a, a, size.x * 0.5f, visitor);
                return;
            }

            Vector2 dir = delta / length;
            Vector2 normal = new Vector2(-dir.y, dir.x);

            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;

            // ---- Compute 4 corners inline (no array)
            Vector2 c0 = a - normal * halfWidth - dir * halfHeight;
            Vector2 c1 = a + normal * halfWidth - dir * halfHeight;
            Vector2 c2 = b - normal * halfWidth + dir * halfHeight;
            Vector2 c3 = b + normal * halfWidth + dir * halfHeight;

            float minX = Mathf.Min(c0.x, c1.x, c2.x, c3.x);
            float minY = Mathf.Min(c0.y, c1.y, c2.y, c3.y);
            float maxX = Mathf.Max(c0.x, c1.x, c2.x, c3.x);
            float maxY = Mathf.Max(c0.y, c1.y, c2.y, c3.y);

            int2 minCell = WorldToCell(new Vector2(minX, minY));
            int2 maxCell = WorldToCell(new Vector2(maxX, maxY));

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    int2 cell = new int2(x, y);

                    if (!IsInsideGrid(cell))
                        continue;

                    Vector2 worldPos = CellToWorld(cell);
                    Vector2 ap = worldPos - a;

                    float localY = Vector2.Dot(ap, dir);
                    float localX = Vector2.Dot(ap, normal);

                    bool inside =
                        localY >= -halfHeight &&
                        localY <= length + halfHeight &&
                        Mathf.Abs(localX) <= halfWidth;

                    if (!inside)
                        continue;

                    visitor.VisitCell(x, y);

                    var list = cells[x, y].entities;
                    if (!list.IsCreated)
                        continue;

                    for (int i = 0; i < list.Length; i++)
                        visitor.Visit(list[i]);
                }
            }
        }
        
        public void RemoveUnit(int unit, float2 position)
        {
            var cell = WorldToCell(position);
            int x = cell.x;
            int y = cell.y;
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                cells[x, y].occupied = false;

                var entities = cells[x, y].entities;
                if (entities.IsCreated)
                {
                    int length = entities.Length;
                    for (int i = 0; i < length; i++)
                    {
                        if (entities[i] == unit)
                        {
                            int lastIndex = entities.Length - 1;
                            entities[i] = entities[lastIndex];
                            entities.Length--;
                            break;
                        }
                    }

                    cells[x, y].entities = entities;
                }
            }
        }

        public void RemoveUnit(int unit, float2 position, float radius)
        {
            var centerCell = WorldToCell(position);
            int rangeX = (int)(radius / cellSize);
            int rangeY = (int)(radius / cellSize);
            float rsq = radius * radius;
            for (int dx = -rangeX; dx <= rangeX; dx++)
            {
                for (int dy = -rangeY; dy <= rangeY; dy++)
                {
                    int x = centerCell.x + dx;
                    int y = centerCell.y + dy;

                    if (x < 0 || y < 0 || x >= width || y >= height)
                        continue;

                    float2 cellWorldPos = CellToWorldCenter(x, y);
                    if (math.distancesq(cellWorldPos, position) <= rsq)
                    {
                        cells[x, y].occupied = false;

                        var entities = cells[x, y].entities;
                        if (entities.IsCreated)
                        {
                            int length = entities.Length;
                            for (int i = 0; i < length; i++)
                            {
                                if (entities[i] == unit)
                                {
                                    int lastIndex = entities.Length - 1;
                                    entities[i] = entities[lastIndex];
                                    entities.Length--;
                                    break;
                                }
                            }

                            cells[x, y].entities = entities;
                        }
                    }
                }
            }
        }

        public void RemoveUnit(int unit, float2 position, float2 size)
        {
            int2 centerCell = WorldToCell(position);
            float2 halfSize = size * 0.5f;
            int rangeX = (int)(size.x / cellSize);
            int rangeY = (int)(size.y / cellSize);

            for (int dx = -rangeX; dx <= rangeX; dx++)
            {
                for (int dy = -rangeY; dy <= rangeY; dy++)
                {
                    int x = centerCell.x + dx;
                    int y = centerCell.y + dy;

                    if (x < 0 || y < 0 || x >= width || y >= height)
                        continue;

                    float2 cellWorldPos = CellToWorldCenter(x, y);
                    if (math.abs(cellWorldPos.x - position.x) <= halfSize.x &&
                        math.abs(cellWorldPos.y - position.y) <= halfSize.y)
                    {
                        cells[x, y].occupied = false;

                        var entities = cells[x, y].entities;
                        if (entities.IsCreated)
                        {
                            int length = entities.Length;
                            for (int i = 0; i < length; i++)
                            {
                                if (entities[i] == unit)
                                {
                                    int lastIndex = entities.Length - 1;
                                    entities[i] = entities[lastIndex];
                                    entities.Length--;
                                    break;
                                }
                            }

                            cells[x, y].entities = entities;
                        }
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

        public float Radiussq(float radius) => (radius + cellSize) * (radius + cellSize) + float.Epsilon;

        int2 WorldToCell(float2 worldPos) => WorldToCell(worldPos, float2.zero);

        int2 WorldToCell(float2 worldPos, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            var x = (int)math.floor(
                (worldPos.x - (gridCenter.x - halfGrid.x)) / cellSize
            );
            var y = (int)math.floor(
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
            var data = cells[c.x, c.y];
            return IsInsideGrid(c) && !data.occupied && !data.trigger;
        }

        public float2 CellToWorld(int2 cell) => CellToWorld(cell, float2.zero);

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

        public float CellSize => cellSize;

        public void Dispose()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y].entities.Dispose();
                }
            }
        }
    }

    [Serializable]
    public struct Cell
    {
        public bool occupied;
        public bool trigger;
        public NativeList<int> entities;
    }
}