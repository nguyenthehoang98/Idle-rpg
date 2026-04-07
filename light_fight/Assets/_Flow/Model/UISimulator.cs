using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Flow.Model
{
    public class UISimulator
    {
        public static Vector2 GetScreenPointFromRect(RectTransform rect)
        {
            return RectTransformUtility.WorldToScreenPoint(Camera.main, rect.position);
        }

        public static Vector2 GetScreenPointFromSprite(SpriteRenderer sprite)
        {
            return Camera.main.WorldToScreenPoint(sprite.transform.position);
        }

        public static Vector2 GetScreenPointFromWorldPosition(Vector3 worldPosition)
        {
            return Camera.main.WorldToScreenPoint(worldPosition);
        }
        
        public static void Drag(RectTransform rect, Vector2 from, Vector2 to)
        {
            var eventData = new PointerEventData(EventSystem.current);

            var target = rect.gameObject;

            eventData.position = from;
            eventData.pointerPress = target;
            eventData.pointerDrag = target;

            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.beginDragHandler);

            // drag
            eventData.position = to;
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.dragHandler);

            ExecuteEvents.Execute(target, eventData, ExecuteEvents.endDragHandler);
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);
        }
        
        public static bool Click(Vector2 screenPosition)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                GameObject go = results[0].gameObject;
                ExecuteEvents.ExecuteHierarchy(
                    go,
                    eventData,
                    ExecuteEvents.pointerClickHandler
                );
            }
            
            return results.Count > 0;
        }
        
        public static bool Click(RectTransform rect, Camera cam = null)
        {
            Vector3 worldPos = rect.TransformPoint(rect.rect.center);
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            return Click(screenPos);
        }
    }
}