using UnityEngine;

namespace _Game.AbilitySystem
{
    [CreateAssetMenu(menuName = "Ability data")]
    public class AbilityData : ScriptableObject
    {
        public CoreAbilityArg arg;
        public ShapeData shape;
        public TrajectoryArg trajectory;
    }
}