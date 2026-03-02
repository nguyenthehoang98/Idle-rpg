using UnityEngine;

namespace _Game.Battle.AbilitySystem
{
    [CreateAssetMenu(menuName = "Ability SO")]
    public class AbilitySO : ScriptableObject
    {
        public string note;
        public CoreAbilityArg core;
        public ShapeArg shape;
        public TrajectoryArg trajectory;
        public StateModifierArg stateModifier;
    }
}