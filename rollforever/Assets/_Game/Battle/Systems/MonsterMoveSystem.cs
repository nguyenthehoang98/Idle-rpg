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
        
        private EcsPool<Unit> unitPool;
        private EcsFilter ecsFilter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            ecsFilter = world.Filter<Unit>()
                .End();
            unitPool = world.GetPool<Unit>();

            matrix = new MatrixData[width, height];
            cellSize = new float2(1.25f, 1.25f);
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                    matrix[i, j].trigger = false;
            }

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                float2 position = shareData.Simulator.GetAgentPosition(unit.agentId);
                var cell = WorldToCell(position);
                if (cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height)
                {
                    matrix[cell.x, cell.y].trigger = true;
                }
            }

            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                float2 position = shareData.Simulator.GetAgentPosition(unit.agentId);
#if UNITY_EDITOR
                float radius = shareData.Simulator.GetAgentRadius(unit.agentId);
                float2 velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                float neighborDist = shareData.Simulator.GetAgentNeighborDist(unit.agentId);
                Circle2D(position, radius, Color.gray, 12, shareData.TimeDelta);
                Circle2D(position, neighborDist, new Color(0, 1, 1, 0.2f), 12, shareData.TimeDelta);
                Debug.DrawRay((Vector2)position, ((Vector2)velocity).normalized * radius);
#endif
                bool paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;
                
                float2 goal = shareData.Simulator.GetAgentGoal(unit.agentId);
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
#if UNITY_EDITOR
                        //Debug.DrawLine((Vector2)position, (Vector2)cellToWorld, Color.red, shareData.TimeDelta);        
#endif
                    }
                    else
                    {
                        shareData.Simulator.SetAgentGoal(unit.agentId);
                    }
                }
            }

#if UNITY_EDITOR
            /*for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float2 world = CellToWorld(new int2(i, j));
                    Color color = Color.gray;
                    Box2D(world, cellSize, color, shareData.TimeDelta);

                    bool selected = false;
                    if (matrix[i, j].trigger)
                    {
                        color = Color.yellow;
                        selected = true;
                    }
                    if (matrix[i, j].occupied)
                    {
                        color = Color.magenta;
                        selected = true;
                    }
                    
                    if (selected) Box2dSelected(world, cellSize, color, shareData.TimeDelta);
                }
            }*/
#endif
            shareData.Simulator.DoStep();
        }

        public void PostRun(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();
            
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);
                
                bool paused = shareData.Simulator.IsAgentPaused(unit.agentId);
                if (paused)
                    continue;
                
                float2 velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                if (math.lengthsq(velocity) < THREASHOLD_VELOCITYSQ)
                {
                    float2 position = shareData.Simulator.GetAgentPosition(unit.agentId);
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

        bool TryFindCellExpandFromCenter(
            int2 centerCell,
            float2 point,
            out int2 resultCell)
        {
            resultCell = new int2(-1, -1);

            float bestDistToPoint = float.MaxValue;
            int maxDist = math.max(width, height);

            for (int dist = 0; dist <= maxDist; dist++)
            {
                bool foundAtThisDist = false;
                bestDistToPoint = float.MaxValue;

                for (int dx = -dist; dx <= dist; dx++)
                for (int dy = -dist; dy <= dist; dy++)
                {
                    if (math.max(math.abs(dx), math.abs(dy)) != dist)
                        continue;

                    int2 c = new int2(centerCell.x + dx, centerCell.y + dy);

                    if (!IsInsideGrid(c))
                        continue;

                    if (!IsEmpty(c))
                        continue;

                    if (!HasAnyFreeNeighbor8(c))
                        continue;

                    float2 wp = CellToWorld(c);
                    float dToPoint = math.lengthsq(wp - point);

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
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int2 n = new int2(cell.x + dx, cell.y + dy);

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
            float2 halfGrid = HalfGridSize();
            return new float2(
                gridCenter.x - halfGrid.x + (cell.x + 0.5f) * cellSize.x,
                gridCenter.y - halfGrid.y + (cell.y + 0.5f) * cellSize.y
            );
        }
        
        int2 WorldToCell(float2 worldPos, float2 gridCenter)
        {
            float2 halfGrid = HalfGridSize();
            int x = (int)math.floor(
                (worldPos.x - (gridCenter.x - halfGrid.x)) / cellSize.x
            );
            int y = (int)math.floor(
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
        
        static void Circle2D(float2 center, float radius, Color col, int samples, float deltaTime)
        {
            float2 from, to;

            float angleIncrease = (float)(Math.PI * 2) / samples;
            from = to = new float2(center.x + radius * Mathf.Cos(0.0f), center.y + radius * Mathf.Sin(0.0f));

            for (int i = 0; i < samples; i++)
            {
                float rad = angleIncrease * (i + 1);

                to = new float2(center.x + radius * Mathf.Cos(rad), center.y + radius * Mathf.Sin(rad));

                Line(from, to, col, deltaTime);

                from = to;
            }
        }

        static void Box2D(float2 center, float2 size, Color color, float deltaTime)
        {
            float2 half = size * 0.5f;

            Vector3 p1 = new Vector3(center.x - half.x, center.y - half.y, 0);
            Vector3 p2 = new Vector3(center.x + half.x, center.y - half.y, 0);
            Vector3 p3 = new Vector3(center.x + half.x, center.y + half.y, 0);
            Vector3 p4 = new Vector3(center.x - half.x, center.y + half.y, 0);

            Debug.DrawLine(p1, p2, color, deltaTime);
            Debug.DrawLine(p2, p3, color, deltaTime);
            Debug.DrawLine(p3, p4, color, deltaTime);
            Debug.DrawLine(p4, p1, color, deltaTime);
        }

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
        
        static void Line(float2 from, float2 to, Color col, float deltaTime)
        {
            Debug.DrawLine((Vector2) from, (Vector2) to, col, deltaTime);
        }
    }
}