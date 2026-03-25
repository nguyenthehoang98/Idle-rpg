using _KIT.Utils;
using Animancer;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class RangedMonsterAuthoring : MonsterAuthoring
    {
        [SerializeField] private bool shouldRecovery;
        [SerializeField] private Vector3 muzzleOffset;
        
        private Coroutine attackCoroutine;
        private Coroutine recoverCoroutine;

        public Vector3 MuzzleOffset => muzzleOffset;

        public override void PlayAttackAnimation()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            if (recoverCoroutine != null) StopCoroutine(recoverCoroutine);
            
            AnimancerState attack = PlayAnimation(AnimationName.Attack);
            attackCoroutine = this.WaitInvoke(attack.Duration, () =>
            {
                if(shouldRecovery)
                {
                    AnimancerState recovery = PlayAnimation(AnimationName.Recovery);
                    recoverCoroutine = this.WaitInvoke(recovery.Duration, () => { PlayAnimation(AnimationName.Idle); });
                }
                else
                {
                    PlayAnimation(AnimationName.Idle);
                }
            });
        }
    }
}