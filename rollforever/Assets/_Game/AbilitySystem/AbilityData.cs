using UnityEngine;

namespace _Game.AbilitySystem
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