using _Games.Combat.EntityComponentSystem.View;
using _KIT.Utils;
using Animancer;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class RangedMonster : Monster
    {
        [SerializeField] private bool shouldRecovery;
        [SerializeField] private Vector3 muzzleOffset;
        
        private Coroutine attackCoroutine;
        private Coroutine recoverCoroutine;
        
        public override void OnAttack()
        {
            bool flip = transform.localRotation.eulerAngles.y != 0;
            int offset = flip ? -1 : 1;
            EntityCastSkillManager.Instance.Trigger(Entity, muzzleOffset * offset);
        }

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