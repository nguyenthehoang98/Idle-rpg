using _TDS.GameplayScene.SkillSystem;
using UnityEngine;

namespace _TDS.GameplayScene.Unit
{
    public class HeroController : MonoBehaviour
    {
        [SerializeField] private Weapon weapon;
        [SerializeField] private HeroType heroType;

        private int forcedTargetEntity = -1;
        private bool isInitialized;

        public HeroType HeroType => heroType;
        public Weapon Weapon => weapon;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            if (weapon == null) weapon = GetComponentInChildren<Weapon>();
        }

        public void Initialize()
        {
            if (weapon != null && !weapon.IsInitialized)
            {
                weapon.Initialize();
            }
            isInitialized = true;
        }

        public void ForceTarget(int entityId)
        {
            forcedTargetEntity = entityId;
        }

        public void ClearForceTarget()
        {
            forcedTargetEntity = -1;
        }

        public bool HasForcedTarget => forcedTargetEntity >= 0;
        public int ForcedTargetEntity => forcedTargetEntity;
    }
}
