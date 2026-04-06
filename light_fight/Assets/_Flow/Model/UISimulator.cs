using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Flow.Model
{
    public class UISimulator
    {
        public static void Drag(RectTransform rect, Vector2 from, Vector2 to, Camera cam = null)
        {
            var eventData = new PointerEventData(EventSystem.current);

            // START position
            eventData.position = from;

            // Raycast lấy object
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count == 0) return;

            var target = results[0].gameObject;

            // Pointer Down
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerDownHandler);

            // Begin Drag
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.beginDragHandler);

            // Drag (giả lập nhiều bước cho mượt)
            int steps = 10;
            for (int i = 1; i <= steps; i++)
            {
                Vector2 pos = Vector2.Lerp(from, to, i / (float)steps);
                eventData.position = pos;

                ExecuteEvents.Execute(target, eventData, ExecuteEvents.dragHandler);
            }

            // End Drag
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.endDragHandler);

            // Pointer Up
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