using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

        public void OutboundFly()
        {
            rotatePivot.localRotation = Quaternion.Euler(0, 0, 0);
        }

        public void HangFly()
        {
        }

        public void ReturnFly()
        {
            rotatePivot.localRotation = Quaternion.Euler(0, 0, 180);
        }

        public void CompleteFly()
        {
        }
    }
}