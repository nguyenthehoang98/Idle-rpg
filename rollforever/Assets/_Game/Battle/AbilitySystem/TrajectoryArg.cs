using System;
using PathCreation;

namespace _Game.Battle.AbilitySystem
{
    [Serializable]
    public struct TrajectoryArg
    {
        public float delayStart;
        public TrajectoryType type;
        public PathCreator path;
        public VelocityArg velocity;
    }

    [Serializable]
    public struct VelocityArg
    {
        public float acceleration;
        public float speed;
    }

    public enum TrajectoryType
    {
        Teleport,
        Velocity,
        Path,
    }
}