using RVO;

namespace _Game.Battle
{
    public class BattleStartupShareData
    {
        public BattleStartupShareData(Simulator simulator, Matrix matrix, LevelSpawnConfig levelSpawnConfig,
            float timeDelta)
        {
            LevelSpawnConfig = levelSpawnConfig;
            Matrix = matrix;
            TimeDelta = timeDelta;
            Simulator = simulator;
        }
        
        public LevelSpawnConfig LevelSpawnConfig { get; }

        public Simulator Simulator { get; }
        
        public Matrix Matrix { get; }
        
        public float TimeDelta { get; }
    }
}