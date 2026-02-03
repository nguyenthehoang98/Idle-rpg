using System.Collections.Generic;
using _Game.AbilitySystem;
using _KIT.Utils;
using Leopotam.EcsLite;

namespace _Game.Battle.Systems
{
    public class AbilitySystem : IEcsInitSystem, IEcsRunSystem
    {
        private Dictionary<int, List<IAbilityLogic>> skillContainer;
        private List<IAbilityLogic> abilities;
        private Queue<IAbilityLogic> completes;
        
        private BattleStartupShareData shareData;
        
        public void Init(IEcsSystems systems)
        {
            completes = new Queue<IAbilityLogic>();
            abilities = new List<IAbilityLogic>();
            skillContainer = new Dictionary<int, List<IAbilityLogic>>();
            
            shareData = systems.GetShared<BattleStartupShareData>();
        }

        public void Run(IEcsSystems systems)
        {
            float dt = shareData.TimeDelta;
            foreach (var ability in abilities)
            {
                ability.Update(dt);
            }

            foreach (var ability in abilities)
            {
                if(ability.IsCompleted) completes.Enqueue(ability);
            }

            while (completes.Count > 0)
            {
                var item = completes.Dequeue();
                item.Shutdown();
                CollectionUtils.RemoveFast(abilities, item);
            }
        }
    }
}