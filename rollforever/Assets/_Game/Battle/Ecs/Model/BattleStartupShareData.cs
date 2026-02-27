using System;
using System.Collections.Generic;
using _Game.Battle.Level;
using _KIT.Schedule;
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
            Simulator.SetAgentDefaults(15f, 10, 5f, 5f, 2f, 2f, new float2(0f, 0f));
            List<int> obstacles = new List<int>();
            foreach (var point in levelSpawnSo.designConfig.LoopPoints())
            {
                float2 halfSize = levelSpawnSo.designConfig.CellSize * 0.55f;
                List<float2> points = new List<float2>
                {
                    point + new Vector2(-halfSize.x, -halfSize.y),
                    point + new Vector2(-halfSize.x, halfSize.y),
                    point + new Vector2(halfSize.x, halfSize.y),
                    point + new Vector2(halfSize.x, -halfSize.y),
                };
                obstacles.Add(Simulator.AddObstacle(points));
            }

            Obstacles = obstacles.ToArray();
        }

        GameLoop GameLoop { get; }

        public LevelSpawnSO LevelSpawnSo { get; }

        public Simulator Simulator { get; }
        
        public Matrix Matrix { get; }
        
        public float TimeDelta { get; private set; }

        public readonly int[] Obstacles;

        public double Time => GameLoop.Time;

        public void Dispose()
        {
            Simulator.Dispose();
            Matrix.Dispose();
        }
    }
}