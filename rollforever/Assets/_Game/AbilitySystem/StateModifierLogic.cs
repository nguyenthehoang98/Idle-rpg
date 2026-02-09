using System;
using _Game.Battle.Data;
using JetBrains.Annotations;
using Leopotam.EcsLite;
using RVO;
using UnityEngine;

namespace _Game.AbilitySystem
{
    public sealed class StateModifierLogic : IDisposable
    {
        private ModifierGroup group;
        private ISubStateModifier modifier;

        public StateModifierLogic(StateModifierArg arg,
            Simulator simulator, EcsPool<UnitData> unitPool, EcsPool<UnitModifierData> modifierPool,
            EcsPool<UnitPosTempData> unitPosTempPool
        )
        {
            group = arg.group;
            switch (arg.type)
            {
                case StateModifierType.None:
                    modifier = new NoneSubStateModifier();
                    break;
                case StateModifierType.Knockback:
                    modifier = new KnockBackSubStateModifier(
                        arg.value, arg.duration, simulator, unitPool, modifierPool, unitPosTempPool
                    );
                    break;
                default:
#if DEVELOP_MODE
                    throw new Exception($"State Modifier {arg.type} chưa được xác định");
#endif
                    modifier = new NoneSubStateModifier();
                    break;
            }
        }

        public void Startup(int entity)
        {
            modifier.Startup(entity);
        }

        public void Update(float deltaTime)
        {
            modifier.Update(deltaTime);
        }

        public void Shutdown()
        {
            modifier.Shutdown();
        }

        public void TriggerTarget(int target)
        {
            if (group == ModifierGroup.Target) modifier.Trigger(target);
        }

        public void TriggerTeammate(int target)
        {
            if (group == ModifierGroup.Teammate) modifier.Trigger(target);
        }

        public void TriggerSelf(int target)
        {
            if (group == ModifierGroup.Self) modifier.Trigger(target);
        }

        public void OnEntityDestroyed(int entity)
        {
            modifier.OnEntityDestroyed(entity);
        }
        
        public void Dispose()
        {
        }
    }

    interface ISubStateModifier
    {
        void Startup(int entity);
        void Trigger(int target);
        void Update(float dt);
        void OnEntityDestroyed(int entity);
        void Shutdown();
    }
}