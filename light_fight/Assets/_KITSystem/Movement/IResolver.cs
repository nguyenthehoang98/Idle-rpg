using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Movement
{
    public interface IResolver
    {
        void Initialize();
        List<Vector3> Resolve(List<Vector3> positions, List<bool> alives, List<Vector3> desiredVelocities);
    }
}