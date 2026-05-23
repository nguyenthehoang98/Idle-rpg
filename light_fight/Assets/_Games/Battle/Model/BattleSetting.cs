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
        public int totalDice;
        public float diceCooldown;
        public float diceDelayTrigger;
        [TitleGroup("Weapon")]
        public float weaponCooldown;
        public float weaponAttackRange;
        [TitleGroup("View")]
        public ISlotView slot;
        public SkillFrameConfig skillFrameConfig;
    }
}