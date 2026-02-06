using Geometry;
using UnityEngine;

namespace _Game.AbilitySystem
{
    [CreateAssetMenu(menuName = "Ability data")]
    public class AbilityData : ScriptableObject
    {
        public CoreAbilityArg arg;
        public Shape shape;
        public TrajectoryArg trajectory;
        public ModifierArg modifier;
    }
}