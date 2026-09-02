using System;
using _GameToolkit.ResourceManagement;
using _TDS.GameConfig;
using _TDS.Gameplay.Utils;
using _TDS.Utils;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay.View
{
    public struct CardItemData
    {
        public WeaponUpgradeData Current;
        public WeaponUpgradeData[] Satellites;
        public bool[] IsUpgraded;
        public int currentGroup;
    }

    public class CardItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtName;
        [SerializeField] private TextMeshProUGUI txtDesc;
        [SerializeField] private PowerItem[] items;
        [SerializeField] private Button button;

        public event Action<WeaponUpgradeData> OnSelected; 
        
        private void Awake()
        {
            button.onClick.AddListener(() =>
            {
                OnSelected?.Invoke(upgradeData);
            });
        }

        private WeaponUpgradeData upgradeData;

        public async UniTask Init(CardItemData itemData)
        {
            upgradeData = itemData.Current;
           
            txtName.text = $"Weapon {upgradeData.id} " + LocalizeManager.GetPowerLevel(itemData.currentGroup + 1);
            
            txtDesc.text = LocalizeManager.GetUpgradeWeaponLocalize(upgradeData).ToString();
            
            var satellites = itemData.Satellites;
            for (int i = 0; i < satellites.Length; i++)
            {
                PowerItem item = items[i];
                WeaponUpgradeData data = satellites[i];
                item.imgIcon.sprite = await AssetLoader.GetAssetCached<Sprite>(data.iconName);

                if (data.Equals(upgradeData))
                {
                    item.imgBackground.color = Const.POWER_COLOR_2;
                }
                else if (itemData.IsUpgraded[i])
                {
                    item.imgBackground.color = Const.POWER_COLOR_1;
                }
                else
                {
                    item.imgBackground.color = Color.clear;
                }
            }
        }
    }
}