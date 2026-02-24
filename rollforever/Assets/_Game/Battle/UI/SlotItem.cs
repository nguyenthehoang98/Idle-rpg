using UnityEngine;

namespace _Game.Battle.UI
{
    public class SlotItem : MonoBehaviour
    {
        [SerializeField] private GameObject equipmentContainer;
        [SerializeField] private Vector2 slotSize = new Vector2(80, 80);

        EquipmentSlotItem currentSlotItem;
        
        public bool IsEquipped => currentSlotItem != null;
        
        public void Push(EquipmentSlotItem equipmentSlotItem)
        {
            currentSlotItem = equipmentSlotItem;
            RectTransform rect = equipmentSlotItem.GetComponent<RectTransform>();
            Vector2 scale = new Vector2(slotSize.x / rect.rect.width, slotSize.y / rect.rect.height);
            equipmentSlotItem.transform.SetParent(equipmentContainer.transform);
            equipmentSlotItem.transform.localPosition = Vector3.zero;
            equipmentSlotItem.transform.localScale = scale; 
        }
    }
}