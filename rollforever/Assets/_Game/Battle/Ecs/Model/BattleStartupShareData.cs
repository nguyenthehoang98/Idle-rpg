using System;
using _Game.Battle.Level;
using _KIT.Schedule;
using RVO;
using Unity.Mathematics;

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
            Simulator.SetAgentDefaults(1f, 10, 20f, 20f, 1.5f, 5f, float2.zero);
        }

        GameLoop GameLoop { get; }

        public LevelSpawnSO LevelSpawnSo { get; }

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