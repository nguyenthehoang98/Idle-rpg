using System;
using _KIT.Schedule;
using RVO;
using Unity.Mathematics;

namespace _Game.Battle
{
    public class BattleStartupShareData : IDisposable
    {
        public BattleStartupShareData(
            GameLoop gameLoop,
            Simulator simulator, Matrix matrix, LevelSpawnConfig levelSpawnConfig)
        {
            GameLoop = gameLoop;
            LevelSpawnConfig = levelSpawnConfig;
            Matrix = matrix;
            Simulator = simulator;
            TimeDelta = GameLoop.FrameDeltaTime;
            Simulator.SetTimeStep(TimeDelta);
            Simulator.SetAgentDefaults(1f, 10, 20f, 20f, 1.5f, 5f, float2.zero);
        }

        GameLoop GameLoop { get; }

        public LevelSpawnConfig LevelSpawnConfig { get; }

        public Simulator Simulator { get; }
        
        public Matrix Matrix { get; }
        
        public float TimeDelta { get; private set; }

        public double Time => GameLoop.Time;

        public void Dispose()
        {
            Simulator.Dispose();
            Matrix.Dispose();
        }
    }
}