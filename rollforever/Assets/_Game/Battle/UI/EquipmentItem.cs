using System;
using System.Collections.Generic;
using _Game.Scripts.Configs;
using _Game.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _Game.Battle.UI
{
    public class EquipmentItem : DragAndDropBehavior
    {
        [SerializeField] private Image imgIcon;
        [SerializeField] private TextMeshProUGUI textLevel;

        private List<RaycastResult> results = new List<RaycastResult>();
        private SlotItem slotItem;
        private WeaponConfig.WeaponData weaponData;
        private Action onPickEquipment;
        private int weaponLevel;

        public WeaponConfig.WeaponData WeaponData => weaponData;
        public int WeaponLevel => weaponLevel;

        public void Init(Sprite sprite, WeaponConfig.WeaponData weaponData, int weaponLevel, Action pickEquipmentCallback)
        {
            this.onPickEquipment = pickEquipmentCallback;
            this.imgIcon.sprite = sprite;
            this.textLevel.SetText(weaponLevel.ToString());
            this.weaponData = weaponData;
            this.weaponLevel = weaponLevel;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (slotItem != null)
            {
                slotItem.SetOrderCanvas(10);
            }
        }

        protected override bool EndDrop(PointerEventData eventData)
        {
            EventSystem.current.RaycastAll(eventData, results);
            foreach (RaycastResult r in results)
            {
                if (r.gameObject == gameObject) continue;
                EquipmentItem item = r.gameObject.GetComponent<EquipmentItem>();
                if (item != null)
                {
                    if (item.WeaponData.WeaponId == WeaponData.WeaponId &&
                        item.WeaponLevel == WeaponLevel)
                    {
                        item.Init(imgIcon.sprite, weaponData, weaponLevel + 1, null);
                        OnPickEquipment();
                        ResetOrderCanvas();
                        Object.Destroy(gameObject);
                        return true;
                    }

                    return false;
                }
            }
            
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            RaycastHit2D hit = Physics2D.BoxCast(worldPos, new Vector2(0.5f, 0.5f), 0, Vector2.zero);
            if (hit.collider != null)
            {
                SlotItem sl = hit.collider.gameObject.GetComponent<SlotItem>();
                if (sl != null && !sl.IsEquipped)
                {
                    slotItem = sl;
                    sl.Push(this);
                    OnPickEquipment();
                    return true;
                }
            }

            return false;
        }

        void OnPickEquipment()
        {
            if (onPickEquipment != null)
            {
                onPickEquipment();
                onPickEquipment = null;
            }
        }

        void ResetOrderCanvas()
        {
            if (slotItem != null)
            {
                slotItem.SetOrderCanvas(0);
            }
        }
    }
}