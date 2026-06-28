using System;
using _Game.Configs;
using _Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class CardItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txt;
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

        public void Init(WeaponUpgradeData upgradeData)
        {
            this.upgradeData = upgradeData;
            txt.text = $"Weapon {upgradeData.id}\n" +
                       LocalizeManager.GetUpgradeWeaponLocalize(upgradeData);
        }
    }
}