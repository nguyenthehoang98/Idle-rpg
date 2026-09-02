using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class EquipmentSlot : MonoBehaviour
    {
        private static readonly int SlotPush = Animator.StringToHash("slot_push");
        private static readonly int SlotIdle = Animator.StringToHash("slot_idle");
        private static readonly int SlotRelease = Animator.StringToHash("slot_release");
        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");

        [SerializeField] private Animator animator;
        [SerializeField] private Image content;

        private Coroutine coroutine;
        
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
            if (coroutine != null) StopCoroutine(coroutine);
            content.material.SetFloat(Dissolve, 0);
        }

        public void SetDissolve(float duration)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(LerpDissolve(duration));
        }
        
        public Vector3 Position => content.transform.position;

        public void ResetIcon() => content.enabled = false;

        private IEnumerator LerpDissolve(float duration)
        {
            if (duration <= 0)
            {
                content.material.SetFloat(Dissolve, 1);
                yield break;
            }
            
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                float dt = Time.deltaTime;
                elapsedTime += dt;
                
                float t = Mathf.Clamp01(elapsedTime / duration);
                content.material.SetFloat(Dissolve, t);

                yield return null;
            }
        }
    }
}