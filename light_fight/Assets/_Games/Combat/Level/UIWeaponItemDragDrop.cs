using System.Collections.Generic;
using _Games.Config;
using _Games.Misc;
using _Games.Misc.Model;
using _Games.Utils;
using _KIT.Pool;
using _KIT.Resource;
using Unity.Mathematics;
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
            
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            var hits = Physics2D.BoxCastAll(worldPos, new Vector2(1f, 1f), 0, Vector2.zero);
            SlotItem result = null;
            float distance = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    SlotItem sl = hit.collider.gameObject.GetComponent<SlotItem>();
                    if (sl != null)
                    {                   
                        Vector2 position = sl.transform.position;
                        float d = math.distancesq(worldPos, position);
                        if (d < distance)
                        {
                            distance = d;
                            result = sl;
                        }
                    }
                }
            }

            if (result != null && !result.IsEquipped)
            {
                result.Equip(weaponData, weaponRarity);
                KitPool.Destroy(gameObject);
                onPickWeapon?.Invoke();
                onPickWeapon = null;
                return true;
            }
            else if (result != null && result.IsEquipped && result.WeaponData.WeaponId == weaponData.WeaponId
                     && result.WeaponRarity == weaponRarity && weaponRarity < RarityMethod.MaxRarity)
            {
                result.Equip(weaponData, RarityMethod.IncreaseRarity(weaponRarity));
                KitPool.Destroy(gameObject);
                onPickWeapon?.Invoke();
                onPickWeapon = null;
                return true;
            }

            return false;
        }
    }
}