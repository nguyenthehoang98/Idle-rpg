using System;
using System.Collections.Generic;
using _GameToolkit.Shared;
using _TDS.GameConfig;
using _TDS.Gameplay.Utils;
using _Toolkit.ResourceManagement;
using UnityEngine;

namespace _TDS.Gameplay.View
{
    public class EquipmentQueue : MonoBehaviour
    {
        [SerializeField] private float slotPushDuration = 0.5f;
        [SerializeField] private ImageAnimation smokeAnimation;
        [SerializeField] private EquipmentSlot[] slots;

        public event Action OnQueueFull;
        public event Action OnFill;

        private List<int> list = new List<int>(); // dánh sách equipment đang cơh̀
        private int[] array = new int[4]; // lưu trữ stack
        private List<int> temp = new List<int>(); // use 1 frame
        private Dictionary<int, WeaponData> container = new Dictionary<int, WeaponData>();
        private List<int> equipments = new List<int>(); // dánh sah eqm mặc dịnh

        public void Init(WeaponConfig config, int[] allEquipments)
        {
            foreach (var sl in slots) sl.ResetIcon();
            
            foreach (var weaponId in allEquipments)
            {
                if (config.TryGetWeaponData(weaponId, out var data))
                {
                    container.Add(weaponId, data);
                    equipments.Add(weaponId);
                }
                else Debug.LogError($"Not found equipment '{weaponId}'");
            }

#if UNITY_EDITOR
            Debug.Log($"[EquipmentQueue] Init all'{allEquipments.Length}', equipment'{equipments.Count}'");
#endif
            
            array = new int[equipments.Count];

            for (int i = 0; i < Const.MAX_WEAPON_SLOT; i++)
            {
                int idx = i % equipments.Count;
                list.Add(equipments[idx]);
            }
        }

        public void Increase()
        {
            if (list.Count == Const.MAX_WEAPON_SLOT)
            {
                return;
            }

            temp = new List<int>(equipments);

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] >= 3) temp.RemoveAt(i);
            }
            
            int idx = RandomUtils.Range(0, temp.Count);

            int item = temp[idx];
            
            list.Add(item);

            array[idx]++;
            
            Vector3 pos = slots[list.Count - 1].Position;

            smokeAnimation.transform.position = pos;
            
            smokeAnimation.Play();
          
            RefreshUI();
            
            OnFill?.Invoke();

            if (list.Count >= Const.MAX_WEAPON_SLOT)
            {
                OnQueueFull?.Invoke();
            }
        }

        public void PushAnimation(int idx, float speed)
        {
            EquipmentSlot slot = slots[idx];
            slot.Speed = speed;
            slot.PlayPush();
            
            Timing.CallDelayed(slotPushDuration / speed, () =>
            {
                slot.ResetIcon();
            });
        }

        public List<int> GetAllEquipment() => list;

        public void Clear()
        {
            Array.Clear(array, 0, array.Length);
            list.Clear();
        }

        public void Decrease(float duration)
        {
            float d = 0.35f;
            if (list.Count > 0)
            {
                int item = list[0];
                int idx = equipments.IndexOf(item);
                array[idx]--;
                list.RemoveAt(0);
                Timing.CallDelayed(duration, () => { slots[0].SetDissolve(0.25f); });
            }

            Timing.CallDelayed(duration + d, RefreshUI);
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
                
                if (!string.IsNullOrEmpty(weaponData.iconName))
                {
                    slots[i].SetIcon(await AssetLoader.GetAssetCached<Sprite>(weaponData.iconName));                
                }
            }
            
            for (int i = 0; i < Const.MAX_WEAPON_SLOT; i++)
            {
                if (i < list.Count) continue;

                slots[i].ResetIcon();
            }
        }
    }
}