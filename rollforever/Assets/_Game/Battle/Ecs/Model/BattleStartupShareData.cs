using System;
using System.Collections.Generic;
using _Game.Battle.Level;
using _KIT.Schedule;
using Geometry;
using RVO;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Ecs.Model
{
    public class BattleStartupShareData : IDisposable
    {
        public BattleStartupShareData(
            GameLoop gameLoop,
            Simulator simulator, Matrix matrix, LevelSpawnSO levelSpawnSo)
        {
            GameLoop = gameLoop;
            LevelSpawnSo = levelSpawnSo;
            Matrix = matrix;
            Simulator = simulator;
            TimeDelta = GameLoop.FrameDeltaTime;
            Simulator.SetTimeStep(TimeDelta);
            Simulator.SetAgentDefaults(5f, 5, 1f, 1f, 2f, 2f, new float2(0f, 0f));
            List<int> obstacles = new List<int>();
            Vector2 cellSize = levelSpawnSo.designConfig.CellSize;
            Vector2[] allPoints = levelSpawnSo.designConfig.LoopPoints();
            float2[] pointsConvert = new float2[allPoints.Length];
            for (var i = 0; i < allPoints.Length; i++)
            {
                Vector2 point = allPoints[i];
                float2 halfSize = cellSize * 0.55f;
                List<float2> points = new List<float2>
                {
                    point + new Vector2(-halfSize.x, -halfSize.y),
                    point + new Vector2(-halfSize.x, halfSize.y),
                    point + new Vector2(halfSize.x, halfSize.y),
                    point + new Vector2(halfSize.x, -halfSize.y),
                };
                obstacles.Add(Simulator.AddObstacle(points));
                pointsConvert[i] = point;
            }

            Obstacles = obstacles.ToArray();
            
            GeometryUtils.CalculateBounds(pointsConvert, cellSize * 1.1f, out float2 boxSize);
            BoxSize = boxSize;
        }

        GameLoop GameLoop { get; }

        public LevelSpawnSO LevelSpawnSo { get; }

        public Simulator Simulator { get; }
        
        public Matrix Matrix { get; }
        
        public float TimeDelta { get; private set; }

        public readonly int[] Obstacles;
        
        public Vector2 BoxSize { get; }

        public double Time => GameLoop.Time;

        public void Dispose()
        {
            Simulator.Dispose();
            Matrix.Dispose();
        }
    }
}