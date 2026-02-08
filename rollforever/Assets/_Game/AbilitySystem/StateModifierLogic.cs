using System;

namespace _Game.AbilitySystem
{
    public sealed class StateModifierLogic : IDisposable
    {
        private ISubStateModifier modifier;

        public StateModifierLogic(StateModifierArg arg)
        {
            switch (arg.type)
            {
                case StateModifierType.None:
                    modifier = new NoneSubStateModifier();
                    break;
                
                default:
#if DEVELOP_MODE
                    throw new Exception($"State Modifier {arg.type} chưa được xác định");     
#endif
                    modifier = new NoneSubStateModifier();
                    break;
            }
        }
        
        public void Startup()
        {
        }

        public void Update(float deltaTime)
        {
        }

        public void Shutdown()
        {
        }

        public void Dispose()
        {
        }
    }

    interface ISubStateModifier
    {
        void Execute(float dt);
    }
}