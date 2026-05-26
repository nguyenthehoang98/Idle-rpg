using _Games.Battle.View;
using _KITSystem.SkillSystem.Config;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Games.Battle.Model
{
    [CreateAssetMenu]
    public class BattleSetting : SerializedScriptableObject
    {
        public bool gizmos;
        public float2 center;
        [TitleGroup("Slot")]
        public int totalSlot = 4;
        public float slotCooldownTime = 1;
        public float slotRecoveryTime = 0.5f;
        public float slotLerpColorDuration = 0.1f;
        public int slotMinOffsetSpeed = 70;
        public int slotMaxOffsetSpeed = 170;
        public Color slotColorMinSpeed = Color.red;
        public Color slotColorDefaultSpeed = Color.yellow;
        public Color slotColorMaxSpeed = Color.green;
        [TitleGroup("Weapon")]
        public float weaponCooldown = 1f;
        public float weaponAttackRange = 6f;
        public float weaponRotateDuration = 0.2f;
        public int[] weaponXRotates = new int[6] { 0, 0, 0, 180, 180, 180 };
        [TitleGroup("Attractor")]
        public float attractorFlyTime = 1f;
        [Range(1.1f, 2.0f)] public float minAttractorRadius = 1.1f;
        [Range(1.1f, 2.0f)] public float maxAttractorRadius = 2.0f;
        [TitleGroup("View")]
        public ISlotView slot;
        public IDiceView dice;
        public IWeaponView weapon;
        public IAttractorView attractor;
        public IDiceControlView diceControl;
        public SkillFrameConfig skillFrameConfig;
    }
}