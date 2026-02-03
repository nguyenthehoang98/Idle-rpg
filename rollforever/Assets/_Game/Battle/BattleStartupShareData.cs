using RVO;

namespace _Game.Battle
{
    public class BattleStartupShareData
    {
        public BattleStartupShareData(Simulator simulator, float timeDelta)
        {
            TimeDelta = timeDelta;
            Simulator = simulator;
            Simulator.SetTimeStep(timeDelta);
        }

        public Simulator Simulator { get; }
        
        public float TimeDelta { get; }
    }
}