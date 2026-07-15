using UnityEngine;

namespace _Game.GamePlay.Model
{
    [RequireComponent(typeof(Projectile))]
    public class FlyWeapon : BaseWeapon
    {
        [SerializeField] private Animator animator;
        
        protected override bool IsFlyWeapon => true;

        protected override void OnPlayAttack()
        {
            base.OnPlayAttack();
            
            animator.enabled = false;
            
            ExecuteAttack();
        }

        public override void OnStopAttack()
        {
            base.OnStopAttack();
            
            animator.enabled = true;
        }

        public void OutboundFly() { }
        public void HangFly() { }
        public void ReturnFly() { }
        public void CompleteFly() { }
    }
}