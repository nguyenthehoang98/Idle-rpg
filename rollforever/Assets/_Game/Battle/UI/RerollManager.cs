using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Battle.Ecs.Events;
using _Game.Battle.Ecs.Systems;
using _Game.Scripts.Configs;
using _Game.Scripts.Model;
using _KIT.Config;
using _KIT.Event;
using _KIT.Schedule;
using _KIT.Utils;
using UnityEngine;

namespace _Game.Battle.UI
{
    public class RerollManager : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        [SerializeField] private GameLoop gameLoop;
        [SerializeField] private RerollItem[] itemsView;

        private Dictionary<StatType, BuffConfig.BuffData> allValue = new Dictionary<StatType, BuffConfig.BuffData>();
        private Dictionary<int, int> mapSlotEquipment = new Dictionary<int, int>();
        private BuffConfig buffConfig;

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<NextWaveEvent>(OnNextWave);
            EventBus.Instance.Subscribe<EquipEquipmentEvent>(OnEquipEquipment);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<NextWaveEvent>(OnNextWave);
            EventBus.Instance.Unsubscribe<EquipEquipmentEvent>(OnEquipEquipment);
        }

        private void OnEquipEquipment(EquipEquipmentEvent e)
        {
            mapSlotEquipment[e.SlotId] = e.WeaponId;
        }

        private void OnNextWave(NextWaveEvent e)
        {
            gameLoop.Pause();
            Ecs.Systems.AbilitySystem system = e.Systems.GetSystem<Ecs.Systems.AbilitySystem>();
            system.ClearAll();

            Action<BuffConfig.BuffData> onComplete = (BuffConfig.BuffData buffData) =>
            {
                allValue[buffData.StatType] = buffData;
                List<BuffConfig.BuffData> buffDatas = allValue.Values.ToList();
                e.Systems.GetSystem<PlayerCasterSystem>().UpgradeStat(buffDatas);
            
                e.OnCompleted();
                container.SetActive(false);
            
                gameLoop.Resume();
            };
            ShowReroll(1, onComplete);
        }

        private void ShowEquipment()
        {
        }

        // Nâng cấp chỉ số các slot. 
        private void ShowReroll(int buffLevel, Action<BuffConfig.BuffData> onSelect)
        {
            var list = GetAllBuffs(buffLevel);
            CollectionUtils.Shuffle(ref list);
            for (int i = 0; i < itemsView.Length; i++)
            {
                itemsView[i].Show(list[i], onSelect);
            }

            container.SetActive(true);
        }

        List<BuffConfig.BuffData> GetAllBuffs(int buffLevel)
        {
            if (buffConfig == null) buffConfig = KitConfigManager.Get<BuffConfig>();
            HashSet<(int, int)> hashSet = new HashSet<(int, int)>();        
            List<BuffConfig.BuffData> buffs = new List<BuffConfig.BuffData>();
            if (buffConfig.Find(-1, buffLevel, out var list))
            {
                foreach (var buffData in list)
                {
                    if (hashSet.Add((buffData.BuffId, buffData.BuffLevel)))
                    {
                        buffs.Add(buffData);
                    }
                }
            }

            var values = mapSlotEquipment.Values;
            HashSet<int> equipments = new HashSet<int>();
            foreach (var value in values)
                equipments.Add(value);
            foreach (var equipment in equipments)
            {
                if (buffConfig.Find(equipment, buffLevel, out list))
                {
                    foreach (var buffData in list)
                    {
                        if (hashSet.Add((buffData.BuffId, buffData.BuffLevel)))
                        {
                            buffs.Add(buffData);
                        }
                    }
                }
            }
        
            return buffs;
        }
    }
}