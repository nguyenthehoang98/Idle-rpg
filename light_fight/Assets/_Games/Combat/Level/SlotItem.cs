using UnityEngine;

namespace _Games.Combat.Level
{
    public class SlotItem : MonoBehaviour
    {
        [SerializeField] private Transform icon;

        public Transform Icon => icon;
    }
}