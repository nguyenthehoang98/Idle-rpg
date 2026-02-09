using System;
using PathCreation;

namespace _Game.AbilitySystem
{
    [Serializable]
    public struct TrajectoryArg
    {
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