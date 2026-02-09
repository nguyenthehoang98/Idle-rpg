using _Game.Battle.Data;
using Leopotam.EcsLite;
using RVO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.AbilitySystem
{
    class KnockBackSubStateModifier : ISubStateModifier
    {
        private Simulator simulator;
        private EcsPool<UnitData> unitPool;

        private NativeList<Data> list;
        private int entity;
        private float force;
        private float duration;

        public KnockBackSubStateModifier(float force, float duration,
            Simulator simulator, EcsPool<UnitData> unitPool
        )
        {
            this.force = force;
            this.duration = duration;
            this.list = new NativeList<Data>(Allocator.Persistent);
            this.simulator = simulator;
            this.unitPool = unitPool;
        }

        public void Startup(int entity)
        {
            this.entity = entity;
        }

        public void Trigger(int target)
        {
            if (target == entity) return;
            if (!unitPool.Has(target)) return;

            UnitData unitData = unitPool.Get(entity);
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
        }

        public void Update(float dt)
        {
            int length = list.Length;
            for (int i = 0; i < length; i++)
            {
                var data = list[i];
                if (data.elapsed >= duration) continue;
                data.elapsed += dt;
                float f = math.clamp(data.elapsed / duration, 0f, 1f);
                float2 distance = f * force * data.direction;
                float2 delta = distance - data.prevPos;
                data.prevPos = distance;
                list[i] = data;

                UnitData unit = unitPool.Get(data.entity);
                float2 agentPos = simulator.GetAgentPosition(unit.agentId);
                simulator.SetAgentPosition(unit.agentId, agentPos + delta);
            }
        }

        public void Shutdown()
        {
        }

        struct Data
        {
            public int entity;
            public float elapsed;
            public float2 direction;
            public float2 prevPos;
        }
    }
}