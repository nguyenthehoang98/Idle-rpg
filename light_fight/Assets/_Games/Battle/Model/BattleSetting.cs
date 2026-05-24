using _Games.Battle.View;
using _KITSystem.SkillSystem.Config;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle.Model
{
    [CreateAssetMenu]
    public class BattleSetting : SerializedScriptableObject
    {
        public float2 center;
        [TitleGroup("Dice")]
        public int totalSlot = 4;
        public float slotCooldown = 1;
        public float slotLerpDuration = 0.5f;
        [TitleGroup("Weapon")]
        public float weaponCooldown = 0.3f;
        public float weaponAttackRange = 6f;
        [TitleGroup("Attractor")]
        public float attractorFlyTime = 1f;
        [Range(1.1f, 2.0f)] public float minAttractorRadius = 1.1f;
        [Range(1.1f, 2.0f)] public float maxAttractorRadius = 2.0f;
        [TitleGroup("View")]
        public ISlotView slot;
        public IDiceView dice;
        public IAttractorView attractor;
        public SkillFrameConfig skillFrameConfig;
    }
}