using UnityEngine;

namespace _TDS.Gameplay.Model
{
    public class ProjectileWeapon : Weapon
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

            animator.speed = TimeScale * (WeaponData.attackSpeed + CurrentUpgradeData.attackRate);
        }
        
        /*
         * @private. callback animation
         */
        public void ExecuteAnimation()
        {
            if (!IsActivated || !IsAttacking) return;

            ExecuteAttack();
        }

        private void EndAnimation()
        {
            if(DependencyReset == DependencyResetAttack.Animation) StopAttack();
        }

        protected override bool UseWeapon => false;
    }
}