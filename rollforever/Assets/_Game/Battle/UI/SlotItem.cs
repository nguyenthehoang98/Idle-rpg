using UnityEngine;

namespace _Game.Battle.UI
{
    public class SlotItem : MonoBehaviour
    {
        [SerializeField] private GameObject equipmentContainer;
        [SerializeField] private Vector2 slotSize = new Vector2(80, 80);

        EquipmentItem currentItem;
        
        public bool IsEquipped => currentItem != null;
        
        public void Push(EquipmentItem equipmentItem)
        {
            currentItem = equipmentItem;
            RectTransform rect = equipmentItem.GetComponent<RectTransform>();
            Vector2 scale = new Vector2(slotSize.x / rect.rect.width, slotSize.y / rect.rect.height);
            equipmentItem.transform.SetParent(equipmentContainer.transform);
            equipmentItem.transform.localPosition = Vector3.zero;
            equipmentItem.transform.localScale = scale; 
        }
    }
}