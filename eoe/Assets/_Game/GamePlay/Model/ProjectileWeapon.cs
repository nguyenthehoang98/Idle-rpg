using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class ProjectileWeapon : BaseWeapon
    {
        private static readonly int AttackAnimator = Animator.StringToHash("Attack");
        
        [SerializeField] private Animator animator;
        
        /*
         * @Abstract
         */
        protected override void OnPlayAttack()
        {
            base.OnPlayAttack();
            
            animator.Play(AttackAnimator, 0, 0);

            animator.speed = TimeScale * (WeaponData.attackSpeed + CurrentUpgradeData.attackSpeed);
        }
        
        /*
         * @private. callback animation
         */
        public void ExecuteAnimation()
        {
            if (!IsActivated || !IsAttacking) return;

            ExecuteAttack();
        }

        private void EndAnimation() => OnStopAttack();

        protected override bool UseWeapon => false;
    }
}