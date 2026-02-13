using _KIT.Schedule;
using RVO;

namespace _Game.Battle
{
    public class BattleStartupShareData
    {
        public BattleStartupShareData(
            GameLoop gameLoop,
            Simulator simulator, Matrix matrix, LevelSpawnConfig levelSpawnConfig,
            float timeDelta)
        {
            GameLoop = gameLoop;
            LevelSpawnConfig = levelSpawnConfig;
            Matrix = matrix;
            TimeDelta = timeDelta;
            Simulator = simulator;
        }

        GameLoop GameLoop { get; }

        public LevelSpawnConfig LevelSpawnConfig { get; }

        public Simulator Simulator { get; }
        
        public Matrix Matrix { get; }
        
        public float TimeDelta { get; }

        public double Time => GameLoop.Time;
    }
}