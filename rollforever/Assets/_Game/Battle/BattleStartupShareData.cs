using Geometry;
using RVO;

namespace _Game.Battle
{
    public class BattleStartupShareData
    {
        public BattleStartupShareData(Simulator simulator, Grid<IGridObject> grid, float timeDelta)
        {
            TimeDelta = timeDelta;
            Grid = grid;
            Simulator = simulator;
        }

        public Simulator Simulator { get; }
        public Grid<IGridObject> Grid { get; }
        public float TimeDelta { get; }
    }
}