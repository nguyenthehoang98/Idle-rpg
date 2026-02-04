using System;
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

        private int width = 40;
        private int height = 60;
        private MatrixData[,] matrix;
        private float2 cellSize;

        private EcsPool<UnitData> unitPool;
        private EcsPool<UnitPosData> unitPosPool;
        private EcsFilter ecsFilter;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            ecsFilter = world.Filter<UnitData>()
                .Inc<UnitPosData>()
                .End();
            unitPool = world.GetPool<UnitData>();
            unitPosPool = world.GetPool<UnitPosData>();

            matrix = new MatrixData[width, height];
            cellSize = new float2(1.25f, 1.25f);
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
                var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                var cell = WorldToCell(position);
                if (cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height)
                {
                    matrix[cell.x, cell.y].trigger = true;
                }
            }

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                var position = shareData.Simulator.GetAgentPosition(unit.agentId);
                var paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;

                ref var unitPos = ref unitPosPool.Get(e);
                unitPos.prevPos = unitPos.currentPos;
                unitPos.currentPos = position;

                if (Shape.TryGet(unit.shapeId, out var shape))
                {
                    shape.PrevPosition = unitPos.prevPos;
                    shape.CurrentPosition = unitPos.currentPos;
                }

                var goal = shareData.Simulator.GetAgentGoal(unit.agentId);
                if (ShouldPause(position, goal))
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

        bool ShouldPause(float2 pos, float2 goal)
        {
            return math.distancesq(goal, pos) < math.max(cellSize.x, cellSize.y);
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

        float2 CellToWorld(int2 cell, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            return new float2(
                gridCenter.x - halfGrid.x + (cell.x + 0.5f) * cellSize.x,
                gridCenter.y - halfGrid.y + (cell.y + 0.5f) * cellSize.y
            );
        }

        int2 WorldToCell(float2 worldPos, float2 gridCenter)
        {
            var halfGrid = HalfGridSize();
            var x = (int) math.floor(
                (worldPos.x - (gridCenter.x - halfGrid.x)) / cellSize.x
            );
            var y = (int) math.floor(
                (worldPos.y - (gridCenter.y - halfGrid.y)) / cellSize.y
            );
            return new int2(x, y);
        }

        float2 HalfGridSize()
        {
            return new float2(
                width * cellSize.x * 0.5f,
                height * cellSize.y * 0.5f
            );
        }
    }
}