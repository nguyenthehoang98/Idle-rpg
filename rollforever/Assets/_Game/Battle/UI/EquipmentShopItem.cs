using UnityEngine;
using UnityEngine.EventSystems;

namespace _Game.Battle.UI
{
    public class EquipmentShopItem : DragAndDropBehavior
    {
        protected override bool IsCompleteDrag(PointerEventData eventData, out GameObject dragObject)
        {
            if (RayCast<EquipmentSlotItem>(eventData, out var slotItem))
            {
                dragObject = slotItem.gameObject;
                return true;
            }
            
            dragObject = null;
            return false;
        }

        protected override void CompleteDrag(GameObject targetObject)
        {
            transform.SetParent(targetObject.transform.parent);
            transform.position = targetObject.transform.position;
        }
    }
}