using Unity.Collections;
using Unity.Mathematics;

namespace _KITSystem.Movement
{
    public interface IResolver
    {
        void Initialize();
        
        void Resolve(NativeArray<float2> positions, NativeArray<float2> destinations, NativeArray<float2> desiredVelocities, NativeList<float2> outFinalVelocities);
    }
}