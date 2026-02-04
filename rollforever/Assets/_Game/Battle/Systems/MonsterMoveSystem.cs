using System;
using _Game.Battle.Data;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Systems
{
    public class MonsterMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        [EcsInject] private readonly BattleStartupRuntimeData runtimeData;

        private int width = 40;
        private int height = 60;
        private bool[,] grid;
        private float2 cellSize;
        
        private EcsPool<Unit> unitPool;
        private EcsFilter ecsFilter;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            ecsFilter = world.Filter<Unit>()
                .End();
            unitPool = world.GetPool<Unit>();

            grid = new bool[width, height];
            cellSize = new float2(1.1f, 1.1f);
        }

        public void Run(IEcsSystems systems)
        {
            shareData.Simulator.EnsureCompleted();
            foreach (var e in ecsFilter)
            {
                var unit = unitPool.Get(e);

                if (ShouldPause(
                    shareData.Simulator.GetAgentPosition(unit.agentId),
                    shareData.Simulator.GetAgentGoal(unit.agentId)
                ))
                {
                    var cell = WorldToCell(shareData.Simulator.GetAgentPosition(unit.agentId));
                    grid[cell.x, cell.y] = true;
                    shareData.Simulator.PauseAgent(unit.agentId, true);
                }
                else
                {
                    shareData.Simulator.SetAgentGoal(unit.agentId, GetGoalAvailable());    
                }
                
#if UNITY_EDITOR
                float2 position = shareData.Simulator.GetAgentPosition(unit.agentId);
                float radius = shareData.Simulator.GetAgentRadius(unit.agentId);
                float2 velocity = shareData.Simulator.GetAgentVelocity(unit.agentId);
                float neighborDist = shareData.Simulator.GetAgentNeighborDist(unit.agentId);
                Circle2D(position, radius, Color.gray, 12, shareData.TimeDelta);
                Circle2D(position, neighborDist, Color.cyan, 12, shareData.TimeDelta);
                Debug.DrawRay((Vector2)position, ((Vector2)velocity).normalized * radius);
#endif
            }

#if UNITY_EDITOR
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float2 world = CellToWorld(new int2(i, j));
                    Box2D(world, cellSize, grid[i, j] ? Color.yellow : Color.gray, shareData.TimeDelta);
                }
            }
#endif
            
            shareData.Simulator.DoStep();
        }

        bool ShouldPause(float2 pos, float2 goal)
        {
            return math.distancesq(goal, pos) < 1.5f;
        }

        float2 GetGoalAvailable()
        {
            int2 center = new int2(width / 2, height / 2);
            int maxRadius = math.max(width, height);

            for (int r = 0; r <= maxRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int dy = r - math.abs(dx);

                    int2 c1 = new int2(center.x + dx, center.y + dy);
                    int2 c2 = new int2(center.x + dx, center.y - dy);

                    if (IsEmpty(c1))
                        return CellToWorld(c1);

                    if (dy != 0 && IsEmpty(c2))
                        return CellToWorld(c2);
                }
            }

            return float2.zero;
        }
        
        bool IsInsideGrid(int2 c)
        {
            return c.x >= 0 && c.y >= 0 &&
                   c.x < grid.GetLength(0) &&
                   c.y < grid.GetLength(1);
        }

        bool IsEmpty(int2 c)
        {
            return IsInsideGrid(c) && !grid[c.x, c.y];
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
        
        static void Line(float2 from, float2 to, Color col, float deltaTime)
        {
            Debug.DrawLine((Vector2) from, (Vector2) to, col, deltaTime);
        }
    }
}