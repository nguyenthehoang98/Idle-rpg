using System;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class MonsterAnimation : MonoBehaviour
    {
        // move at transform
        [SerializeField] private Transform root; // play animation scale ,rotate
        [SerializeField] private Transform flip; // flip

        private Vector3 localScale;
        private bool defaultFace = false; // false: left, true: right

        private void Awake()
        {
            localScale = flip.localScale;
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;

            bool right = pos.x < 0;
            if (right != defaultFace)
            {
                defaultFace = right;
                flip.localScale = new Vector3(localScale.x * (right ? -1 : 1), localScale.y, localScale.z);
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
        }

        public void Move()
        {
            // code
        }
    }
}
