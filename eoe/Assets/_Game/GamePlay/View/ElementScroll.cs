using System.Collections.Generic;
using UnityEngine;

namespace _Game.GamePlay.View
{
    public class ElementScroll : MonoBehaviour
    {
        [SerializeField] private ElementTrigger[] elements;

        [SerializeField] private float beginY = -100;
        [SerializeField] private float height = 40;
        [SerializeField] private float speed = 200;

        private readonly Queue<Color> queue = new();

        private float currentOffset;
        private bool isAnimating;

        private void OnValidate()
        {
            for (int i = 0; i < elements.Length; i++)
            {
                RectTransform rect = elements[i].GetComponent<RectTransform>();

                Vector2 pos = rect.anchoredPosition;
                pos.y = beginY + i * height;
                rect.anchoredPosition = pos;
            }
        }

        public void Push(Color color)
        {
            queue.Enqueue(color);
        }

        private void Update()
        {
            if (!isAnimating && queue.Count > 0)
            {
                StartScroll();
            }

            if (!isAnimating)
                return;

            currentOffset = Mathf.MoveTowards(
                currentOffset,
                height,
                speed * Time.deltaTime);

            ApplyPositions();

            if (Mathf.Approximately(currentOffset, height))
            {
                FinishScroll();
            }
        }

        private void StartScroll()
        {
            // item mới nằm ở buffer dưới
            elements[0].SetData(queue.Dequeue());

            currentOffset = 0;
            isAnimating = true;
        }

        private void ApplyPositions()
        {
            for (int i = 0; i < elements.Length; i++)
            {
                RectTransform rect = elements[i].GetComponent<RectTransform>();

                Vector2 pos = rect.anchoredPosition;
                pos.y = beginY + i * height + currentOffset;

                rect.anchoredPosition = pos;
            }
        }

        private void FinishScroll()
        {
            currentOffset = 0;

            // recycle phần tử trên cùng xuống dưới cùng
            ElementTrigger top = elements[^1];

            for (int i = elements.Length - 1; i > 0; i--)
            {
                elements[i] = elements[i - 1];
            }

            elements[0] = top;

            // reset lại vị trí chuẩn
            for (int i = 0; i < elements.Length; i++)
            {
                RectTransform rect = elements[i].GetComponent<RectTransform>();

                Vector2 pos = rect.anchoredPosition;
                pos.y = beginY + i * height;

                rect.anchoredPosition = pos;
            }

            isAnimating = false;
        }
    }
}