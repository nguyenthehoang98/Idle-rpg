using System.Collections.Generic;
using _Game.Battle.View;
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

    public class BattleStartupRuntimeData
    {
        private Dictionary<int, UnitView> unitContainer = new Dictionary<int, UnitView>();

        public void Insert(int unitId, UnitView view)
        {
            unitContainer[unitId] = view;
        }

        public bool TryGet(int unitId, out UnitView view)
        {
            return unitContainer.TryGetValue(unitId, out view);
        }

        public void Remove(int unitId)
        {
            unitContainer.Remove(unitId);
        }
    }
}