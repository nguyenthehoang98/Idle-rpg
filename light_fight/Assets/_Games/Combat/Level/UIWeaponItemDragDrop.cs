using System.Collections.Generic;
using _Games.Config;
using _Games.Misc;
using _Games.Misc.Model;
using _Games.Utils;
using _KIT.Pool;
using _KIT.Resource;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Action = System.Action;

namespace _Games.Combat.Level
{
    public class UIWeaponItemDragDrop : DragAndDropBehavior
    {
        [SerializeField] private Image imgBorder;
        [SerializeField] private Image imgIcon;

        private List<RaycastResult> results = new List<RaycastResult>();
        private WeaponData weaponData;
        private Rarity weaponRarity;
        private Action onPickWeapon;

        public bool IsEqual(int weaponId)
        {
            return weaponData != null && weaponData.WeaponId == weaponId;
        }

        public async void Initialize(WeaponData weaponData, Rarity weaponRarity, Action pickEquipmentCallback)
        {
            WeaponSO weaponSo = await KitLoaded.LoadAsync<WeaponSO>(GlobalsPath.GetWeaponSOPath(weaponData.WeaponId), true);
            RaritySO raritySo = await KitLoaded.LoadAsync<RaritySO>(GlobalsPath.RARITY_SO, true);

            this.weaponRarity = weaponRarity;
            this.weaponData = weaponData;
            onPickWeapon = pickEquipmentCallback;
            imgIcon.sprite = weaponSo.GetIconRarity(weaponRarity);
            imgBorder.sprite = raritySo.GetBorderRarity(weaponRarity);
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
                    if (item.weaponData.WeaponId == weaponData.WeaponId && item.weaponRarity == weaponRarity && weaponRarity < RarityMethod.MaxRarity)
                    {
                        item.Initialize(weaponData, RarityMethod.IncreaseRarity(weaponRarity), null);
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
                    sqv.Equip(weaponData, weaponRarity);
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