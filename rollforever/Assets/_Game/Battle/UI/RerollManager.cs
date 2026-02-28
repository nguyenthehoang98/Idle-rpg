using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Battle.Ecs.Events;
using _Game.Scripts.Configs;
using _Game.Scripts.Model;
using _KIT.Config;
using _KIT.Event;
using _KIT.Schedule;
using _KIT.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.UI
{
    public class RerollManager : MonoBehaviour
    {
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private GameObject container;
        [SerializeField] private GameLoop gameLoop;
        [SerializeField] private RerollItem[] itemsView;

        private Dictionary<StatType, BuffConfig.BuffData> allValue = new Dictionary<StatType, BuffConfig.BuffData>();
        private Dictionary<int, int> mapSlotEquipment = new Dictionary<int, int>();
        private BuffConfig buffConfig;

        private void Awake()
        {
            container.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<WaveCompleteEvent>(OnNextWave);
            EventBus.Instance.Subscribe<WaveUpdateEquipmentEvent>(OnEquipEquipment);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WaveCompleteEvent>(OnNextWave);
            EventBus.Instance.Unsubscribe<WaveUpdateEquipmentEvent>(OnEquipEquipment);
        }

        private void OnEquipEquipment(WaveUpdateEquipmentEvent e)
        {
            mapSlotEquipment[e.SlotId] = e.WeaponId;
        }

        private void OnNextWave(WaveCompleteEvent e)
        {
            int[] wavesChooseBuff = new int[] { 2, 5, 7, 10, 12, 15, 17, 20 };
            if (wavesChooseBuff.Contains(e.Wave))
            {
                Action<BuffConfig.BuffData> onComplete = (BuffConfig.BuffData buffData) =>
                {
                    allValue[buffData.StatType] = buffData;
                    EventBus.Instance.Publish(new WaveChooseBuffEvent(allValue.Values.ToArray()));
                    container.SetActive(false);
                    EventBus.Instance.Publish(new WaveShowChooseEquipmentEvent(1, 4.5f, new float2(0, -1)));
                };
                ShowReroll(1, onComplete);                
            }
            else
            {
                EventBus.Instance.Publish(new WaveShowChooseEquipmentEvent(1, 4.5f, new float2(0, -1)));
            }
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

            Dictionary<int, int>.ValueCollection values = mapSlotEquipment.Values;
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