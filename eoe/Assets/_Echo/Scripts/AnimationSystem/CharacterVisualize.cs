using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class CharacterVisualize : MonoBehaviour
    {
        [SerializeField] private UnitAnimator animator;
        [SerializeField] private new UnitRenderer renderer;

        public UnitAnimator Animator => animator;

        private void Awake()
        {
            animator.OnAnimationStart += AnimationStart;
        }

        private void AnimationStart((Texture defaultTexture, Texture hdrTexture) tuple)
        {
            renderer.SetTexture(tuple.defaultTexture, tuple.hdrTexture, tuple.hdrTexture == null);
        }

        public void Activate(float duration)
        {
            animator.IsPaused = false;
            renderer.Activate(duration);
        }

        public void Deactivate(float duration)
        {
            animator.IsPaused = true;
            renderer.Deactivate(duration);
        }
    }
}