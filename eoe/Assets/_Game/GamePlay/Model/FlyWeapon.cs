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
            
            if (!IsActivated || !IsAttacking) return;
            
            animator.enabled = false;
            
            ExecuteAttack();
            
            Debug.LogError("@execute attack");
        }
    }
}