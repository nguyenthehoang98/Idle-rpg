using System;
using UnityEngine;

namespace _Games.GamePlay
{
    public class Weapon : MonoBehaviour
    {
        private static readonly int ATTACK = Animator.StringToHash("Attack");
        
        [SerializeField] private Transform muzzle;
        [SerializeField] private new SpriteRenderer renderer;
        [SerializeField] private Animator animator;

        private Action onAttack;

        public Color Color
        {
            get => renderer.color;
            set => renderer.color = value;
        }

        public void Attack(Action action)
        {
            onAttack = action;
            animator.Play(ATTACK);
        }

        public void ExecutePrivate()
        {
            if (onAttack != null)
            {
                onAttack.Invoke();
                onAttack = null;
            }
        }
        
        public Vector3 MuzzlePosition => muzzle.position;

        public float EulerAngleZ
        {
            get => transform.eulerAngles.z;
            set => transform.eulerAngles = new Vector3(0,0, value);
        }
    }
}