using UnityEngine;
using UnityEngine.EventSystems;

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

    public void OnDrag(PointerEventData eventData)
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
        if (IsCompleteDrag(eventData, out GameObject dragObject))
        {
            CompleteDrag(dragObject);
        }
        else
        {
            rectTransform.anchoredPosition = originalPos;
        }
    }

    protected virtual bool IsCompleteDrag(PointerEventData eventData, out GameObject dragObject)
    {
        dragObject = null;
        return false;
    }

    protected virtual void CompleteDrag(GameObject targetObject)
    {
    }
}