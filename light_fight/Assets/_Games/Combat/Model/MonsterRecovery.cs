using _KIT.Utils;
using Animancer;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class MonsterRecovery : Monster
    {
        private Coroutine attackCoroutine;
        private Coroutine recoverCoroutine;
        
        public override void OnAttack()
        {
        }

        public override void PlayAttackAnimation()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            if (recoverCoroutine != null) StopCoroutine(recoverCoroutine);
            
            AnimancerState attack = PlayAnimation(AnimationName.Attack);
            attackCoroutine = this.WaitInvoke(attack.Duration, () =>
            {
                AnimancerState recovery = PlayAnimation(AnimationName.Recovery);
                recoverCoroutine = this.WaitInvoke(recovery.Duration, () =>
                {
                    PlayAnimation(AnimationName.Idle);
                });
            });
        }
    }
}