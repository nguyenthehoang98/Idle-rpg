using _KITSystem.SkillSystem.Config;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    [CreateAssetMenu]
    public class BattleSetting : ScriptableObject
    {
        public BattleMode mode = BattleMode.Default;
        public float2 center;
        [TitleGroup("Dice")]
        public int totalDice;
        public float diceCooldown;
        public float diceDelayTrigger;
        [TitleGroup("Weapon")]
        public float weaponCooldown;
        public float weaponAttackRange;
        [TitleGroup("View")]
        public ConeView coneView;
        public SkillFrameConfig skillFrameConfig;
    }
    
    public enum BattleMode {Default, Test}
}