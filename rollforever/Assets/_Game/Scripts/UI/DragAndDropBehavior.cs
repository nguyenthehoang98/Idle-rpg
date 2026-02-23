using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DragAndDropBehavior : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private List<RaycastResult> results = new List<RaycastResult>();
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private Vector2 originalPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rectTransform.anchoredPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            null, // overlay => camera = null
            out Vector2 localPoint);

        offset = rectTransform.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            null,
            out Vector2 localPoint);

        rectTransform.anchoredPosition = localPoint + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // nếu fail thì reset
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

    protected bool RayCast<T>(PointerEventData eventData, out T hitObject)
    {
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var hit in results)
        {
            if (hit.gameObject == gameObject)
                continue;

            T dropZone = hit.gameObject.GetComponent<T>();
            if (dropZone != null)
            {
                hitObject = dropZone;
                return true;
            }
        }
        
        hitObject = default;
        return false;
    }
}