using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Utils;
using _KITSystem.Resource;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class EquipmentQueue : MonoBehaviour
    {
        [SerializeField] private Image[] imgEquipments;

        private Queue<int> queue = new Queue<int>();

        private Dictionary<int, WeaponData> container;
        private List<int> equipments;

        public async UniTask Init(WeaponConfig config, int[] allEquipments)
        {
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
        }

        public void Increase()
        {
            int idx = RandomUtils.Range(0, equipments.Count);
          
            queue.Enqueue(equipments[idx]);
            
            RefreshUI();

            if (queue.Count >= Const.MAX_WEAPON_SLOT)
            {
                queue.Clear();
                
                RefreshUI();
            }
        }

        public void Decrease()
        {
            if (queue.Count > 0) queue.Dequeue();
            
            RefreshUI();
        }

        private async void RefreshUI()
        {
            int index = 0;
            
            foreach (var item in queue)
            {
                container.TryGetValue(item, out var data);
                Sprite icon = await AssetBundleManager.GetAssetCached<Sprite>(data.iconName);
                imgEquipments[index].sprite = icon;
                index++;
            }

            for (int i = 0; i < Const.MAX_WEAPON_SLOT; i++)
            {
                imgEquipments[i].enabled = i < index;
            }
        }
    }
}