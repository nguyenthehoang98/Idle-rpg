using UnityEngine;
using UnityEngine.EventSystems;

namespace _Games.Misc
{
    public abstract class DragAndDropBehavior : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform rectTransform;
        private Canvas canvas;
        private Vector2 offset;
        private Vector2 originalPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            originalPos = rectTransform.anchoredPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera, 
                out Vector2 localPoint);
            offset = rectTransform.anchoredPosition - localPoint;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);
            rectTransform.anchoredPosition = localPoint + offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!EndDrop(eventData))
            {
                rectTransform.anchoredPosition = originalPos;
            }
        }

        protected abstract bool EndDrop(PointerEventData eventData);
    }
}