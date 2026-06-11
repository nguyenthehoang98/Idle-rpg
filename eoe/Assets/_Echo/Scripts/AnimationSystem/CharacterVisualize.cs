using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class CharacterVisualize : MonoBehaviour
    {
        [SerializeField] private CharacterAnimator animator;
        [SerializeField] private new CharacterRenderer renderer;

        private void Awake()
        {
            animator.OnAnimationStart += AnimationStart;
        }

        private void AnimationStart((Texture defaultTexture, Texture hdrTexture) tuple)
        {
            renderer.SetTexture(tuple.defaultTexture, tuple.hdrTexture, tuple.hdrTexture == null);
        }

        public int PlayAnimation(AnimState state, Direction8 direction)
        {
            return animator.Play(state, direction);
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