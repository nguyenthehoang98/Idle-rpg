using Geometry;
using RVO;

namespace _Game.Battle
{
    public class BattleStartupShareData
    {
        public BattleStartupShareData(Simulator simulator, Grid<IShapeData> grid, float timeDelta)
        {
            TimeDelta = timeDelta;
            Grid = grid;
            Simulator = simulator;
        }

        public Simulator Simulator { get; }
        public Grid<IShapeData> Grid { get; }
        public float TimeDelta { get; }
    }
}