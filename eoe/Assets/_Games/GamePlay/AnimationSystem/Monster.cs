using System;
using System.Collections;
using UnityEngine;

namespace _Games.GamePlay.AnimationSystem
{
    public class Monster : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private int sortingOrderOffset;
        [Header("Animation")]
        [SerializeField] private AnimationAsset idleAnimationAsset;
        [SerializeField] private AnimationAsset walkAnimationAsset;
        [SerializeField] private AnimationAsset attackAnimationAsset;

        private UnitAnimation unitAnimation;

        private void OnEnable()
        {
            StartCoroutine(AutoSort());

            unitAnimation = new UnitAnimation(spriteRenderer);
            unitAnimation.Import(idleAnimationAsset, State.Idle);
            unitAnimation.Import(walkAnimationAsset, State.Walk);
            unitAnimation.Import(attackAnimationAsset, State.Attack);
            AnimationTickable.Add(unitAnimation);
        }

        private void OnDisable()
        {
            AnimationTickable.Remove(unitAnimation);
            unitAnimation = null;
        }

        public void Initialize()
        {
            unitAnimation.PlayAnimation(State.Walk, DirectionExtensions.GetDirection(transform.position, Vector3.zero));
        }
        
        IEnumerator AutoSort()
        {
            while (true)
            {
                spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100) + sortingOrderOffset;
 
                yield return new WaitForSeconds(1f);
            }
        }
    }
}