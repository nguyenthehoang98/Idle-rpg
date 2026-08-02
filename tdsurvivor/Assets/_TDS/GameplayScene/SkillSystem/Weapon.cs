using _Toolkit.SkillSystem.Core;
using UnityEngine;

namespace _TDS.GameplayScene.SkillSystem
{
    public class Weapon : MonoBehaviour
    {
        public Projectile projectile;

        /*
         * Đăng kí pháe
         */
        public void StopAttack()
        {
        }

        public void OnProjectilePhaseChange(ProjectilePhase phase)
        {
        }
    }
}