using System;
using System.Collections.Generic;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Config;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using _KIT.Resource;
using _KIT.Utils;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenWeaponSelectPopupEvent = _Games.Combat.Event.OpenWeaponSelectPopupEvent;

namespace _Games.Combat.View
{
    public partial class WeaponSelectPopup : MonoBehaviour
    {
        [SerializeField] private WeaponItemView itemViewPrefab;
        [SerializeField] private RectTransform content;
        [SerializeField] private Data[] weapons;

        [Header("Tween")] 
        [SerializeField] private MMF_Player openFeedback;
        [SerializeField] private MMF_Player closeFeedback;
        
        [Header("Button")]
        [SerializeField] private Button btnResume;
        
        private readonly List<WeaponData> allData = new List<WeaponData>();        
        
        public static async void Instantiate(Transform parent)
        {
            // Sau sửa lại vào base popup
            GameObject go = await KitLoaded.LoadAsync<GameObject>("WeaponSelectPopup");
            var instance = Instantiate(go, parent).GetComponent<WeaponSelectPopup>();
            instance.Reload();
            instance.OpenPopup(new OpenWeaponSelectPopupEvent());
        }
        
        private void Awake()
        {
            btnResume.onClick.AddListener(() =>
            {
                EventBus.Instance.Publish(new CloseWeaponSelectPopupEvent());
                Close(() =>
                {
                    EventBus.Instance.Publish(new WaveResumeEvent());                        
                });
            });
        }

        public void Reload()
        {
            WeaponConfig weaponConfig = KitConfigManager.Get<WeaponConfig>();
            int[] weaponIds = new int[] { 2001, 2002, 2003, 2010 };
            foreach (var id in weaponIds)
            {
                if(weaponConfig.Find(id, out WeaponData weaponData))
                    allData.Add(weaponData);
            }
        }
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<OpenWeaponSelectPopupEvent>(OpenPopup);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<OpenWeaponSelectPopupEvent>(OpenPopup);
        }
        
        private void OpenPopup(OpenWeaponSelectPopupEvent e)
        {
            Debug.Log("Open popup");
            // tính toán dữ liệu & fill vào data (weapons)
            PickWeapon();
            openFeedback.PlayFeedbacks();;
        }

        private void Close(Action onClosed)
        {
            closeFeedback.PlayFeedbacks();
            this.WaitInvoke(closeFeedback.TotalDuration, () =>
            {
                ReturnPool();
                onClosed?.Invoke();
                content.gameObject.SetActive(false);
            });
        }
        
        private void PickWeapon()
        {
            // Lấy weapon từ pool: vũ khí, máu, giáp ...
            // Random level dựa trên wave hiện tại và level hiện tại
            int length = Mathf.Min(weapons.Length, allData.Count);
            for (int i = 0; i < length; i++)
            {
                WeaponData weaponData = allData[i];
                int level = FormulaUtils.RandomEquipmentLevel(1, 1, 1);
                Data data = weapons[i];
                data.textPrice.SetText("X" + weaponData.Price(level));
                data.textTitle.SetText(weaponData.WeaponName);

                if (data.WeaponItemView == null)
                {
                    var instance = Instantiate(itemViewPrefab, data.container);
                    instance.transform.SetAsFirstSibling();
                    instance.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                    data.WeaponItemView = instance;
                }
                
                data.WeaponItemView.Initialize(weaponData, level, () =>
                {
                    data.textPrice.SetText(String.Empty);
                    data.textTitle.SetText(String.Empty);
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
    }
    
    public partial class WeaponSelectPopup
    {
        [Serializable]
        class Data
        {
            public Transform container;
            public TextMeshProUGUI textTitle;
            public TextMeshProUGUI textPrice;
            public WeaponItemView WeaponItemView { get; set; }
            public int Price { get; set; }
        }
    }
}