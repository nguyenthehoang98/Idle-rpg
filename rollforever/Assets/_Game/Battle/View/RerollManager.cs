using System;
using System.Collections.Generic;
using _Game.Battle.Events;
using _Game.Battle.Systems;
using _Game.Configs;
using _KIT.Config;
using _KIT.Event;
using _KIT.Schedule;
using _KIT.Utils;
using UnityEngine;

public class RerollManager : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private GameLoop gameLoop;
    [SerializeField] private RerollItemView[] itemsView;

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
        AbilitySystem system = e.Systems.GetSystem<AbilitySystem>();
        system.ClearAll();

        Action onComplete = () =>
        {
            e.OnCompleted();
            container.SetActive(false);
        };
        ShowReroll(1, onComplete);
    }

    private void ShowReroll(int buffLevel, Action onSelect)
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