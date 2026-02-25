using _Game.Scripts.Configs;
using _Game.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Game.Battle.UI
{
    public class EquipmentItem : DragAndDropBehavior
    {
        [SerializeField] private Image imgIcon;
        
        private WeaponConfig.WeaponData weaponData;
        private int weaponLevel;

        public void Init(Sprite sprite, WeaponConfig.WeaponData weaponData, int weaponLevel)
        {
            this.imgIcon.sprite = sprite;
            this.weaponData = weaponData;
            this.weaponLevel = weaponLevel;
        }
        
        protected override bool IsCompleteDrag(PointerEventData eventData, out GameObject dragObject)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            RaycastHit2D hit = Physics2D.BoxCast(worldPos, new Vector2(0.5f, 0.5f), 0, Vector2.zero);
            if (hit.collider != null)
            {
                SlotItem slotItem = hit.collider.gameObject.GetComponent<SlotItem>();
                if (slotItem != null && !slotItem.IsEquipped)
                {
                    dragObject = hit.collider.gameObject;
                    return true;
                }
            }

            dragObject = null;
            return false;
        }

        protected override void CompleteDrag(GameObject targetObject)
        {
            SlotItem slotItem = targetObject.GetComponent<SlotItem>();
            slotItem.Push(this);
        }
    }
}