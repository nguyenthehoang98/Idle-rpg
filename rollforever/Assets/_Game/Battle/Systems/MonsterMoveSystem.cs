using _Game.Battle.Data;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem, IEcsPostRunSystem
    {
        struct MatrixData
        {
            public bool occupied;
            public bool trigger;
        }

        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;

        private const float THREASHOLD_VELOCITYSQ = 0.3f;

        private int width = 100;
        private int height = 120;
        private MatrixData[,] matrix;
        private float cellSize;

        private EcsPool<UnitData> unitPool;
        private EcsFilter ecsFilter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            ecsFilter = world.Filter<UnitData>()
                .Exc<DeadFlag>()
                .End();
            unitPool = world.GetPool<UnitData>();

            matrix = new MatrixData[width, height];
            cellSize = 0.5f;
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                    matrix[i, j].trigger = false;
            }

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);

                if (Shape.TryGet(unit.shapeId, out var shape))
                {
                    var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                    var cell = WorldToCell(position);

                    if (cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height)
                    {
                        matrix[cell.x, cell.y].trigger = true;
                    }

                    if (shape.Type == ShapeType.Circle)
                    {
                        var radius = shape.Radius;
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
#if UNITY_EDITOR
                                    Box2dSelected((Vector2) cellWorldPos, new float2(cellSize, cellSize), unit.color, shareData.TimeDelta);
#endif
                                }
                            }
                        }
                    }
                    else if (shape.Type == ShapeType.Box)
                    {
                        
                    }
                }
            }

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                var goal = shareData.Simulator.GetAgentGoal(unit.agentId);
                var radius = shareData.Simulator.GetAgentRadius(unit.agentId);
                var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                var prefPosition = shareData.Simulator.GetAgentPrefPosition(unit.agentId);
                
                if (Shape.TryGet(unit.shapeId, out var shape))
                {
                    shape.PrefPosition = prefPosition;
                    shape.CurrentPosition = position;
                }

                if (ShouldPause(position, goal, radius))
                {
                    Pause(unit.agentId, position);
                }
                else
                {
                    var found = TryFindCellExpandFromCenter(WorldToCell(goal), position, out var cell);
                    if (found)
                    {
                        var cellToWorld = CellToWorld(cell);
                        shareData.Simulator.SetAgentGoal(unit.agentId, cellToWorld);
                    }
                    else
                    {
                        shareData.Simulator.SetAgentGoal(unit.agentId);
                    }
                }
            }

            shareData.Simulator.DoStep();
        }

        public void PostRun(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);

                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                var velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                if (math.lengthsq(velocity) < THREASHOLD_VELOCITYSQ)
                {
                    var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                    Pause(unit.agentId, position);
                }
            }
        }

        void Pause(int agentId, float2 position)
        {
            var cell = WorldToCell(position);
            if (cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height)
            {
                matrix[cell.x, cell.y].occupied = true;
            }

            shareData.Simulator.PauseAgent(agentId, true);
        }

        bool ShouldPause(float2 pos, float2 goal, float radius)
        {
            return math.distancesq(goal, pos) < (radius + cellSize) * (radius + cellSize);
        }

        bool TryFindCellExpandFromCenter(int2 centerCell, float2 point, out int2 resultCell)
        {
            resultCell = new int2(-1, -1);

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

                    var c = new int2(centerCell.x + dx, centerCell.y + dy);

                    if (!IsInsideGrid(c))
                        continue;

                    if (!IsEmpty(c))
                        continue;

                    if (!HasAnyFreeNeighbor8(c))
                        continue;

                    var wp = CellToWorld(c);
                    var dToPoint = math.lengthsq(wp - point);

                    if (!foundAtThisDist || dToPoint < bestDistToPoint)
                    {
                        foundAtThisDist = true;
                        bestDistToPoint = dToPoint;
                        resultCell = c;
                    }
                }

                if (foundAtThisDist)
                    return true;
            }

            return false;
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

        bool IsInsideGrid(int2 c)
        {
            return c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;
        }

        bool IsEmpty(int2 c)
        {
            var data = matrix[c.x, c.y];
            return IsInsideGrid(c) && !data.occupied && !data.trigger;
        }

        float2 CellToWorld(int2 cell) => CellToWorld(cell, float2.zero);

        int2 WorldToCell(float2 worldPos) => WorldToCell(worldPos, float2.zero);

        float2 CellToWorldCenter(int x, int y) => CellToWorldCenter(x, y, float2.zero);
        
        float2 CellToWorldCenter(int x, int y, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            return new float2(
                gridCenter.x - halfGrid.x + (x + 0.5f) * cellSize,
                gridCenter.y - halfGrid.y + (y + 0.5f) * cellSize
            );
        }

        float2 CellToWorld(int2 cell, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            return new float2(
                gridCenter.x - halfGrid.x + (cell.x + 0.5f) * cellSize,
                gridCenter.y - halfGrid.y + (cell.y + 0.5f) * cellSize
            );
        }

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
        
        static void Box2dSelected(float2 center, float2 size, Color color, float deltaTime)
        {
            float2 half = size * 0.5f;

            Vector3 p1 = new Vector3(center.x - half.x, center.y - half.y, 0);
            Vector3 p2 = new Vector3(center.x + half.x, center.y - half.y, 0);
            Vector3 p3 = new Vector3(center.x + half.x, center.y + half.y, 0);
            Vector3 p4 = new Vector3(center.x - half.x, center.y + half.y, 0);

            Debug.DrawLine(p1, p3, color, deltaTime);
            Debug.DrawLine(p2, p4, color, deltaTime);
        }
    }
}