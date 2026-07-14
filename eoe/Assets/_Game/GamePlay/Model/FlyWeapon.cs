using UnityEngine;

namespace _Game.GamePlay.Model
{
    [RequireComponent(typeof(Projectile))]
    public class FlyWeapon : BaseWeapon
    {
        protected override bool IsFlyWeapon => true;

        protected override void OnPlayAttack()
        {
            base.OnPlayAttack();
            
            ExecuteAttack();
        }
    }
}