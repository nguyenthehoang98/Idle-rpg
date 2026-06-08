using System;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class TestAnimatorControl : MonoBehaviour
    {
        public SpriteAnimator animator;

        private AnimState state;
        private Direction8 direction = Direction8.B;

        private void Awake()
        {
            animator.OnOneShotAnimationEnd += animState =>
            {
                Debug.Log($"OnOneShotAnimationEnd: {animState}");
            };
        }

        private void Update()
        {
            int horizontal = (int)Input.GetAxisRaw("Horizontal");
            int vertical = (int)Input.GetAxisRaw("Vertical");

            bool isMoving = horizontal != 0 || vertical != 0;

            if (isMoving)
            {
                state = AnimState.Walk;

                direction = GetDirection(horizontal, vertical);

                animator.Play(state, direction);
            }
            else
            {
                state = AnimState.Idle;

                // Giữ nguyên hướng cuối cùng
                animator.Play(state, direction);
            }

            // Test attack
            if (Input.GetKeyDown(KeyCode.Space))
            {
                animator.Play(AnimState.Attack1, direction);
            }
        }

        private Direction8 GetDirection(int horizontal, int vertical)
        {
            if (horizontal == 0 && vertical == 1)
                return Direction8.T;

            if (horizontal == 1 && vertical == 1)
                return Direction8.TR;

            if (horizontal == 1 && vertical == 0)
                return Direction8.R;

            if (horizontal == 1 && vertical == -1)
                return Direction8.BR;

            if (horizontal == 0 && vertical == -1)
                return Direction8.B;

            if (horizontal == -1 && vertical == -1)
                return Direction8.BL;

            if (horizontal == -1 && vertical == 0)
                return Direction8.L;

            if (horizontal == -1 && vertical == 1)
                return Direction8.TL;

            return direction;
        }
    }
}