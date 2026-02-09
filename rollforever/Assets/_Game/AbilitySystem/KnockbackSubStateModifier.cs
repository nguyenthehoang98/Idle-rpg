using System.Diagnostics;
using _Game.Battle.Data;
using Leopotam.EcsLite;
using RVO;
using Unity.Collections;
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    class KnockBackSubStateModifier : ISubStateModifier
    {
        private Simulator simulator;
        private EcsPool<UnitData> unitPool;
        private EcsPool<UnitModifierData> modifierPool;

        private NativeList<Data> list;
        private int unit;
        private float force;
        private float duration;

        public KnockBackSubStateModifier(float force, float duration,
            Simulator simulator, EcsPool<UnitData> unitPool, EcsPool<UnitModifierData> modifierPool
        )
        {
            this.force = force;
            this.duration = duration;
            this.list = new NativeList<Data>(Allocator.Persistent);
            this.simulator = simulator;
            this.unitPool = unitPool;
            this.modifierPool = modifierPool;
        }

        public void Startup(int entity)
        {
            this.unit = entity;
        }

        public void Trigger(int target)
        {
            if (target == unit) return;
            if (!unitPool.Has(target)) return;

            UnitData unitData = unitPool.Get(unit);
            float2 pos = simulator.GetAgentPosition(unitData.agentId);
            UnitData targetData = unitPool.Get(target);
            float2 targetPos = simulator.GetAgentPosition(targetData.agentId);
            float2 direction = float2.zero;
            if (math.lengthsq(targetPos - pos) <= 0)
            {
            }
            else
            {
                direction = math.normalize(targetPos - pos);
            }
            
            int length = list.Length;
            for (int i = 0; i < length; i++)
            {
                if (list[i].entity == target)
                {
                    list[i] = new Data
                    {
                        entity = target,
                        elapsed = 0,
                        direction = direction
                    };
                    return;
                }
            }

            list.Add(new Data
            {
                entity = target,
                elapsed = 0,
                direction = direction
            });

            ref var modifier = ref modifierPool.Get(target);
            StatusEffectMethod.Add(ref modifier.effect, StatusEffect.KnockBack);
        }

        public void Update(float dt)
        {
            int length = list.Length;
            for (int i = 0; i < length; i++)
            {
                var data = list[i];
                if (data.hasDied) continue;
                if (data.elapsed >= duration) continue;
                data.elapsed += dt;
                float f = math.clamp(data.elapsed / duration, 0f, 1f);
                float2 distance = f * force * data.direction;
                float2 delta = distance - data.prevDistance;
                data.prevDistance = distance;
                list[i] = data;

                UnitData unitData = unitPool.Get(data.entity);
                float2 agentPos = simulator.GetAgentPosition(unitData.agentId);
                simulator.SetAgentPosition(unitData.agentId, agentPos + delta);
            }
        }

        public void Shutdown()
        {
            int length = list.Length;
            for (int i = 0; i < length; i++)
            {
                int e = list[i].entity;
                var data = list[i];
                if (data.hasDied) continue;
                
                ref var modifier = ref modifierPool.Get(e);
                StatusEffectMethod.Remove(ref modifier.effect, StatusEffect.KnockBack);

                if (modifier.effect.Has(StatusEffect.Stun)) continue;
                if (modifier.effect.Has(StatusEffect.KnockBack)) continue;

                UnitData unitData = unitPool.Get(e);
                simulator.PauseAgent(unitData.agentId, false);

                float d = math.length(data.prevDistance);
                if(d >= force)
                    UnityEngine.Debug.Log(d);
            }
        }

        public void OnEntityDestroyed(int entity)
        {
            int length = list.Length;
            for (int i = 0; i < length; i++)
            {
                var data = list[i];
                if(data.entity == entity)
                {
                    data.hasDied = true;
                    list[i] = data;
                    return;
                }
            }
        }

        struct Data
        {
            public int entity;
            public float elapsed;
            public float2 direction;
            public float2 prevDistance;
            public bool hasDied;
        }
    }
}