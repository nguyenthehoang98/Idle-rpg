using System;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Utils;
using _KITSystem.Resource;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class EquipmentQueue : MonoBehaviour
    {
        private static readonly int SlotPush = Animator.StringToHash("slot_push");

        [SerializeField] private float slotPushDuration = 0.5f;
        [SerializeField] private Animator[] animators = new Animator[4];
        [SerializeField] private Image[] imgEquipments;

        public event Action OnQueueFull;

        private List<int> list; //key
        private int[] slots;
        private Dictionary<int, WeaponData> container;
        private List<int> equipments;

        public async UniTask Init(WeaponConfig config, int[] allEquipments)
        {
            list = new List<int>();
            equipments = new List<int>();
            container = new Dictionary<int, WeaponData>();

            foreach (var img in imgEquipments)
            {
                img.enabled = false;
            }

            foreach (var weaponId in allEquipments)
            {
                if (config.TryGetWeaponData(weaponId, out var data))
                {
                    await AssetBundleManager.GetAssetCached<Sprite>(data.iconName);

                    container.Add(weaponId, data);
                    equipments.Add(weaponId);
                }
            }
            
            slots = new int[equipments.Count];
        }

        public void Increase()
        {
            NativeList<int> temp = new NativeList<int>(slots.Length, Allocator.Temp);
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] < 3) temp.Add(i);
            }
            
            int idx = RandomUtils.Range(0, temp.Length);

            temp.Dispose();

            slots[idx]++;
            
            int item = equipments[idx];
            
            list.Add(item);
          
            RefreshUI();

            if (list.Count >= Const.MAX_WEAPON_SLOT)
            {
                OnQueueFull?.Invoke();
            }
        }

        public void PushAnimation(int idx)
        {
            animators[idx].Play(SlotPush);
         
            this.WaitInvoke(slotPushDuration, () =>
            {
                imgEquipments[idx].enabled = false;
            });
        }
        
        public void Clear()
        {
            Array.Clear(slots, 0, slots.Length);
            list.Clear();
        }

        public void Decrease()
        {
            if (list.Count > 0) list.RemoveAt(0);
            
            RefreshUI();
        }

        public bool TryGetWeaponData(int idx, out WeaponData weaponData)
        {
            if (idx < list.Count)
            {
                return container.TryGetValue(list[idx], out weaponData);
            }
            else
            {
                weaponData = default;
                return false;
            }
        }

        private async void RefreshUI()
        {
            for (int i = 0; i < list.Count; i++)
            {
                TryGetWeaponData(i, out WeaponData weaponData);
                Sprite icon = await AssetBundleManager.GetAssetCached<Sprite>(weaponData.iconName);
                imgEquipments[i].sprite = icon;
            }
            
            for (int i = 0; i < Const.MAX_WEAPON_SLOT; i++)
            {
                imgEquipments[i].enabled = i < list.Count;
            }
        }
    }
}