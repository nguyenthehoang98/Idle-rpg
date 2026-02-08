using RVO;

namespace _Game.Battle
{
    public class BattleStartupShareData
    {
        public BattleStartupShareData(Simulator simulator, Matrix matrix, float timeDelta)
        {
            Matrix = matrix;
            TimeDelta = timeDelta;
            Simulator = simulator;
        }

        public Simulator Simulator { get; }
        
        public Matrix Matrix { get; }
        
        public float TimeDelta { get; }
    }
}