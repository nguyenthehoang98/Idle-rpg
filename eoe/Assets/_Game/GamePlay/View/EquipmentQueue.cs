using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Utils;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class EquipmentQueue : MonoBehaviour
    {
        [SerializeField] private float slotPushDuration = 0.5f;
        [SerializeField] private UIAnimation smokeAnimation;
        [SerializeField] private EquipmentSlot[] slots;

        public event Action OnQueueFull;
        public event Action OnFill;

        private readonly List<int> list = new List<int>(); // dánh sách equipment đang cơh̀
        private int[] array = new int[4]; // lưu trữ stack
        private readonly List<int> temp = new List<int>(); // use 1 frame
        private Dictionary<int, WeaponData> container = new Dictionary<int, WeaponData>();
        private readonly List<int> equipments = new List<int>(); // dánh sah eqm mặc dịnh

        public async UniTask Init(WeaponConfig config, int[] allEquipments)
        {
            foreach (var sl in slots) sl.ResetIcon();
            
            foreach (var weaponId in allEquipments)
            {
                if (config.TryGetWeaponData(weaponId, out var data))
                {
                    await AssetBundleManager.GetAssetCached<Sprite>(data.iconName);

                    container.Add(weaponId, data);
                    equipments.Add(weaponId);
                }
            }
            
            array = new int[equipments.Count];

            for (int i = 0; i < Const.MAX_WEAPON_SLOT; i++)
            {
                int idx = i % equipments.Count;
                list.Add(equipments[idx]);
            }
            
            OnQueueFull?.Invoke();
        }

        public void Increase()
        {
            temp.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < 3) temp.Add(i);
            }
            
            int idx = RandomUtils.Range(0, temp.Count);

            array[idx]++;
            
            int item = equipments[idx];
            
            list.Add(item);

            smokeAnimation.transform.position = slots[list.Count - 1].Position;
            
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
            
            this.WaitInvoke(slotPushDuration / speed, () =>
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
                this.WaitInvoke(duration, () => { slots[0].SetDissolve(0.25f); });
            }

            this.WaitInvoke(duration + d, RefreshUI);
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
                slots[i].SetIcon(await AssetBundleManager.GetAssetCached<Sprite>(weaponData.iconName));
            }
            
            for (int i = 0; i < Const.MAX_WEAPON_SLOT; i++)
            {
                if (i < list.Count) continue;

                slots[i].ResetIcon();
            }
        }
    }
}