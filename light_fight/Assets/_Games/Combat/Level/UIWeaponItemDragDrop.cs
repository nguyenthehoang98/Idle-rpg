using System.Collections.Generic;
using _Games.Config;
using _Games.Misc;
using _Games.Utils;
using _KIT.Pool;
using _KIT.Resource;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Action = System.Action;

namespace _Games.Combat.Level
{
    public class UIWeaponItemDragDrop : DragAndDropBehavior
    {
        [SerializeField] private Image imgIcon;
        [SerializeField] private TextMeshProUGUI textLevel;

        private List<RaycastResult> results = new List<RaycastResult>();
        private WeaponData weaponData;
        private int weaponLevel;
        private Action onPickWeapon;

        public bool IsEqual(int weaponId)
        {
            return weaponData != null && weaponData.WeaponId == weaponLevel;
        }

        public async void Initialize(WeaponData weaponData, int weaponLevel, Action pickEquipmentCallback)
        {
            WeaponSO so = await KitLoaded.LoadAsync<WeaponSO>(GlobalsPath.GetWeaponSOPath(weaponData.WeaponId));

            this.weaponLevel = weaponLevel;
            this.weaponData = weaponData;
            onPickWeapon = pickEquipmentCallback;
            imgIcon.sprite = so.GetIcon(weaponLevel);
            textLevel.text = weaponLevel.ToString();
        }

        protected override bool EndDrop(PointerEventData eventData)
        {
            EventSystem.current.RaycastAll(eventData, results);
            foreach (RaycastResult r in results)
            {
                if (r.gameObject == gameObject) continue;
                var item = r.gameObject.GetComponent<UIWeaponItemDragDrop>();
                if (item != null)
                {
                    if (item.weaponData.WeaponId == weaponData.WeaponId && item.weaponLevel == weaponLevel)
                    {
                        item.Initialize(weaponData, weaponLevel + 1, null);
                        onPickWeapon?.Invoke();
                        onPickWeapon = null;
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
                    sqv.Equip(weaponData, weaponLevel);
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