using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class EquipmentSlot : MonoBehaviour
    {
        private static readonly int SlotPush = Animator.StringToHash("slot_push");
        private static readonly int SlotIdle = Animator.StringToHash("slot_idle");
        private static readonly int SlotRelease = Animator.StringToHash("slot_release");

        [SerializeField] private Animator animator;
        [SerializeField] private Image content;

        public float Speed
        {
            set => animator.speed = value;
        }

        public void PlayIdle() => animator.Play(SlotIdle);
        public void PlayPush() => animator.Play(SlotPush);
        public void PlayRelease() => animator.Play(SlotRelease);

        public void SetIcon(Sprite sprite)
        {
            content.sprite = sprite;
            content.enabled = true;
        }
        
        public Vector3 Position => content.transform.position;

        public void ResetIcon() => content.enabled = false;
    }
}