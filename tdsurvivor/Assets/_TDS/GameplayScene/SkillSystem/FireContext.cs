using UnityEngine;

namespace _TDS.GameplayScene.SkillSystem
{
    public struct FireContext
    {
        public Vector3 EquipmentPivot;
        public Vector3 EquipmentMuzzle;
        public Vector3 ProjectileDestination;
        public bool UseEquipment;
        public Weapon Equipment;
    }
}