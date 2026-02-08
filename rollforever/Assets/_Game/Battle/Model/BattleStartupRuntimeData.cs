using System.Collections.Generic;
using _Game.Battle.View;

namespace _Game.Battle
{
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