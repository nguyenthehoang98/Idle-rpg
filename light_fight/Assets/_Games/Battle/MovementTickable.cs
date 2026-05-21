using _KITSystem.Movement;
using _KITSystem.Schedule;
using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class MovementTickable : ITickable
    {
        [SerializeField] private MPU mpu = new MPU();

        public MPU MPU => mpu;

        public void Tick(float deltaTime) => mpu.Tick(deltaTime);
    }
}