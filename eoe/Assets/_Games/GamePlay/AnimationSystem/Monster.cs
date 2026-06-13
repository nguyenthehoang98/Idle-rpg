using System;
using System.Collections;
using _KITSystem.Resource;
using UnityEngine;

namespace _Games.GamePlay.AnimationSystem
{
    public class Monster : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private MonsterData monsterData;
        [Header("Animation")]
        [SerializeField] private AnimationAsset idleAnimationAsset;
        [SerializeField] private AnimationAsset walkAnimationAsset;
        [SerializeField] private AnimationAsset attackAnimationAsset;

        private UnitAnimation unitAnimation;

        public static event Action<Monster> OnMonsterEnable;
        public static event Action<Monster> OnMonsterDisable;

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
            Direction direction = DirectionExtensions.GetDirection(transform.position, Vector3.zero);
            
            unitAnimation.PlayAnimation(State.Walk, direction);
            
            AgentTickable.Add(this);
            
            OnMonsterEnable?.Invoke(this);
        }

        public void Destroy()
        {
            AgentTickable.Remove(this);
            
            OnMonsterDisable?.Invoke(this);
            
            Pool.Destroy(gameObject);
        }
        
        IEnumerator AutoSort()
        {
            while (true)
            {
                spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
 
                yield return new WaitForSeconds(1f);
            }
        }
    }
}