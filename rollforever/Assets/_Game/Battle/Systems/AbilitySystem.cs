using System.Collections.Generic;
using _Game.AbilitySystem;
using _Game.Battle.Events;
using _KIT.Event;
using _KIT.Resource;
using _KIT.Utils;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;

namespace _Game.Battle.Systems
{
    public class AbilitySystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private Dictionary<int, AbilityLogic> skillSource;
        
        [EcsInject] private readonly BattleStartupShareData shareData;
        
        private List<AbilityLogic> abilities;
        private Queue<AbilityLogic> additions;
        private Queue<AbilityLogic> completes;
        
        public async void Init(IEcsSystems systems)
        {
            additions = new Queue<AbilityLogic>();
            completes = new Queue<AbilityLogic>();
            abilities = new List<AbilityLogic>();
            skillSource = new Dictionary<int, AbilityLogic>();

            AbilityData abilityData = await KitLoaded.LoadAsync<AbilityData>("AbilityData");
            skillSource[0] = new AbilityLogic(abilityData);
            
            EventBus.Instance.Subscribe<CastSkillEvent>(OnCastSkillArg);
        }

        private void OnCastSkillArg(CastSkillEvent e)
        {
            if (skillSource.TryGetValue(e.SkillId, out var abilityLogic))
            {
                var item = abilityLogic.CreateInstance();
                item.Startup(e.StartPosition, e.Target);
                additions.Enqueue(item);
            }
        }

        public void Run(IEcsSystems systems)
        {
            while (additions.Count > 0)
            {
                var item = additions.Dequeue();
                abilities.Add(item);
            }
            
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

        public void Destroy(IEcsSystems systems)
        {
            EventBus.Instance.Unsubscribe<CastSkillEvent>(OnCastSkillArg);
        }
    }
}