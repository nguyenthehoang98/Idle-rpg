using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime;
using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class SkillTickable : ITickable
    {
        [SerializeField] private SPU spu = new SPU();
        
        public SPU SPU => spu;

        public void Tick(float deltaTime) => spu.Tick(deltaTime);
    }
}