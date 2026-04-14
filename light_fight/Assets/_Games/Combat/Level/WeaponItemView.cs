using System;
using System.Collections.Generic;
using _Games.Combat.Event;
using _Games.Config;
using _Games.Misc;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Action = System.Action;

namespace _Games.Combat.Level
{
    public class WeaponItemView : DragAndDropBehavior
    {
        [SerializeField] private Button btnInfo;
        [SerializeField] private Image imgIcon;
        [SerializeField] private Transform container;
        [SerializeField] private TextMeshProUGUI textLevel;
        
        public WeaponData WeaponData { get; private set; }
        public int WeaponLevel {get; private set;}
        
        private SlotItem slotItem;
        private List<RaycastResult> results = new List<RaycastResult>();
        private Action onPickWeapon;
        
        private void Awake()
        {
            btnInfo.onClick.AddListener(() =>
            {
                EventBus.Instance.Publish(new BattleShowWeaponInfoEvent(WeaponData, WeaponLevel));
            });
        }
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<WaveContinueEvent>(OnWaveContinue);
        } 

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WaveContinueEvent>(OnWaveContinue);
        }
        
        private void OnWaveContinue(WaveContinueEvent e) => container.gameObject.SetActive(false);

        public async void Initialize(WeaponData weaponData, int level, Action pickEquipmentCallback)
        {
            WeaponSO so = await KitLoaded.LoadAsync<WeaponSO>(weaponData.WeaponId.ToString());

            onPickWeapon = pickEquipmentCallback;
            WeaponLevel = level;
            WeaponData = weaponData;

            imgIcon.sprite = so.GetIcon(level);
            textLevel.text = level.ToString();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (slotItem != null) slotItem.SetSortingOrder(10);
        }

        protected override bool EndDrop(PointerEventData eventData)
        {
            EventSystem.current.RaycastAll(eventData, results);
            foreach (RaycastResult r in results)
            {
                if (r.gameObject == gameObject) continue;
                var item = r.gameObject.GetComponent<WeaponItemView>();
                if (item != null)
                {
                    if (item.WeaponData.WeaponId == WeaponData.WeaponId && item.WeaponLevel == WeaponLevel)
                    {
                        item.Initialize(WeaponData, WeaponLevel + 1, null);
                        onPickWeapon?.Invoke();
                        onPickWeapon = null;
                        
                        if (slotItem != null) slotItem.SetSortingOrder(0);
                        KitPool.Destroy(gameObject);
                        
                        return true;
                    }

                    return false;
                }
            }
            
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            RaycastHit2D hit = Physics2D.BoxCast(worldPos, new Vector2(0.5f, 0.5f), 0, Vector2.zero);
            if (hit.collider != null)
            {
                SlotItem sqv = hit.collider.gameObject.GetComponent<SlotItem>();
                if (sqv != null && !sqv.IsEquipped)
                {
                    slotItem = sqv;
                    sqv.Equip(WeaponData, WeaponLevel);
                    KitPool.Destroy(gameObject);
                    onPickWeapon?.Invoke();
                    onPickWeapon = null;
                    return true;
                }
            }

            return false;
        }
    }
}