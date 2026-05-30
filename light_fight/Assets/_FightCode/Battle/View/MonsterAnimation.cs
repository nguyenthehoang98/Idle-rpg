using System;
using Animancer;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class MonsterAnimation : MonoBehaviour
    {
        // move at transform
        [SerializeField] private Transform root; // play animation scale ,rotate
        [SerializeField] private Transform flip; // flip
        [SerializeField] private AnimationClip moveAnimationClip;

        private AnimancerComponent animancer;
        private AnimancerState state;

        private Vector3 localScale;
        private bool defaultFace = true; // false: left, true: right

        private void Awake()
        {
            animancer = GetComponent<AnimancerComponent>();
            localScale = flip.localScale;
        }

        /*private void OnValidate()
        {
            animancer = GetComponent<AnimancerComponent>();
            if (animancer.Animator == null)
                animancer.Animator = GetComponent<Animator>();
        }*/

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;

            bool right = pos.x < 0;
            if (right != defaultFace)
            {
                defaultFace = right;
                flip.localScale = new Vector3(localScale.x * (right ? 1 : -1), localScale.y, localScale.z);
            }
        }

        public void BeHit()
        {
            // spawn fx
        }

        public void Dead(Action onDestroy)
        {
            onDestroy?.Invoke();
        }

        public void OnDeadEvent()
        {
        }

        public void Idle()
        {
            // code
            if (state != null) state.Stop();
        }

        public void Move()
        {
            state = animancer.Play(moveAnimationClip);
            // code
        }
    }
}
