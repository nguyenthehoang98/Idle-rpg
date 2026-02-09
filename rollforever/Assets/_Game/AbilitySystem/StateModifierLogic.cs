using System;
using _Game.Battle.Data;
using Leopotam.EcsLite;
using RVO;

namespace _Game.AbilitySystem
{
    public sealed class StateModifierLogic : IDisposable
    {
        private ModifierGroup group;
        private ISubStateModifier modifier;

        public StateModifierLogic(StateModifierArg arg, Simulator simulator, EcsPool<UnitData> unitPool)
        {
            group = arg.group;
            switch (arg.type)
            {
                case StateModifierType.None:
                    modifier = new NoneSubStateModifier();
                    break;
                case StateModifierType.Knockback:
                    modifier = new KnockBackSubStateModifier(
                        arg.value, arg.duration, simulator, unitPool
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
        
        public void Dispose()
        {
        }
    }

    interface ISubStateModifier
    {
        void Startup(int entity);
        void Trigger(int target);
        void Update(float dt);
        void Shutdown();
    }
}