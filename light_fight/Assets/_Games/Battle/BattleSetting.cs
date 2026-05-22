using System;
using Unity.Mathematics;

namespace _Games.Battle
{
    [Serializable]
    public struct BattleSetting
    {
        public int totalDice;
        public float diceCooldown;
        public float diceDelayTrigger;
        public float weaponCooldown;
        public float weaponAttackRange;
        public float2 center;
    }
}