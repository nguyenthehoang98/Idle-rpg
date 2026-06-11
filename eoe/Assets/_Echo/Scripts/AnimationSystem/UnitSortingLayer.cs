using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class UnitSortingLayer : MonoBehaviour
    {
        [SerializeField] private bool autoSortLayer;
        [SerializeField] private int offset;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private void LateUpdate()
        {
            if (autoSortLayer)
            {
                spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100) + offset;
            }
        }
    }
}