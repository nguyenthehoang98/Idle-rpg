using System;
using System.Collections.Generic;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Config;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using _KIT.Popup;
using _KIT.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Combat.View
{
    public partial class WeaponSelectPopup : PopupBase
    {
        [SerializeField] private WeaponItemView itemViewPrefab;
        [SerializeField] private RectTransform content;
        [SerializeField] private Object[] weapons;
        [Header("Button")]
        [SerializeField] private Button btnResume;
        
        private readonly List<WeaponData> allData = new List<WeaponData>();       
        private readonly List<Data> currentWeapons = new List<Data>();
        
        private void Awake()
        {
            btnResume.onClick.AddListener(Close);
        }

        protected override void OnOpen()
        {
            WeaponConfig weaponConfig = KitConfigManager.Get<WeaponConfig>();
            int[] weaponIds = new int[] { 2001, 2002, 2003, 2010 };
            foreach (var id in weaponIds)
            {
                if(weaponConfig.Find(id, out WeaponData weaponData))
                    allData.Add(weaponData);
            }
            
            // tính toán dữ liệu & fill vào data (weapons)
            currentWeapons.Clear();
            
            this.WaitNextFrame(PickWeapon);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            ReturnPool();
            content.gameObject.SetActive(false);
            EventBus.Instance.Publish(new WaveContinueEvent()); 
        }
        
        private void PickWeapon()
        {
            SkillConfig skillConfig = KitConfigManager.Get<SkillConfig>();
            // Lấy weapon từ pool: vũ khí, máu, giáp ...
            // Random level dựa trên wave hiện tại và level hiện tại
            int length = Mathf.Min(weapons.Length, allData.Count);
            for (int i = 0; i < length; i++)
            {
                WeaponData weaponData = allData[i];
                skillConfig.Find(weaponData.SkillId, out var skillData);
                int level = FormulaUtils.RandomEquipmentLevel(1, 1, 1);
                int price = weaponData.Price(level);
                int power = FormulaUtils.PowerWeapon(weaponData, skillData, level);
                Object @object = weapons[i];
                @object.textPrice.text = "X" + weaponData.Price(level);
                @object.textTitle.text = weaponData.WeaponName;
                
                currentWeapons.Add(new Data
                {
                    weaponId = weaponData.WeaponId,
                    weaponLevel = level,
                    price = price,
                    power = power,
                });

                if (@object.WeaponItemView == null)
                {
                    var instance = Instantiate(itemViewPrefab, @object.container);
                    instance.transform.SetAsFirstSibling();
                    instance.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                    @object.WeaponItemView = instance;
                }
                
                @object.WeaponItemView.Initialize(weaponData, level, () =>
                {
                    @object.textPrice.text = String.Empty;
                    @object.textTitle.text = String.Empty;
                });
            }
        }
        
        private void ReturnPool()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                var eqm = weapons[i];
                if (eqm.WeaponItemView == null || eqm.WeaponItemView.transform.parent != eqm.container)
                {
                    weapons[i].WeaponItemView = null;
                }
            }
        }
        
        public IReadOnlyList<Data> AIGetCurrentWeapons() => currentWeapons;

        public bool AIFindWeaponButton(int weaponId, out RectTransform rect)
        {
            foreach (var o in weapons)
            {
                if (o.WeaponItemView != null && o.WeaponItemView.WeaponData.WeaponId == weaponId)
                {
                    rect = o.WeaponItemView.GetComponent<RectTransform>();
                    return true;
                }
            }
            rect = null;
            return false;
        }
    }
    
    public partial class WeaponSelectPopup
    {
        [Serializable]
        class Object
        {
            public Transform container;
            public TextMeshProUGUI textTitle;
            public TextMeshProUGUI textPrice;
            public WeaponItemView WeaponItemView { get; set; }
            public int Price { get; set; }
        }
        
        [Serializable]
        public struct Data
        {
            public int weaponId;
            public int weaponLevel;
            public int price;
            public int power;
        }
    }
}