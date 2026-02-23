using UnityEngine;

namespace _Game.Battle.AbilitySystem
{
    [CreateAssetMenu(menuName = "Ability data")]
    public class AbilityData : ScriptableObject
    {
        public CoreAbilityArg core;
        public ShapeArg shape;
        public TrajectoryArg trajectory;
        public StateModifierArg stateModifier;
    }
}