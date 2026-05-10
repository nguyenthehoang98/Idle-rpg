using TMPro;
using UnityEngine;

namespace _Games.Battle
{
    public class LevelCone : MonoBehaviour
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private Transform pivot;

        public void Init(int order)
        {
            transform.localRotation = Quaternion.Euler(0, 0, -60 * order);
            highlight.gameObject.SetActive(true);
        }
    }
}