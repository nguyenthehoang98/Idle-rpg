using _KITSystem.EventBus;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime.Signal;

namespace _Games.Battle
{
    // [Don't remove]
    public sealed class AgentManager : AgentGrid, ITickable
    {
        protected override void OnInitialize()
        {
            SystemBus.Subscribe<SquareShapeHitSignal>(OnSquareShapeHit);
            SystemBus.Subscribe<CircleShapeHitSignal>(OnCircleShapeHit);
        }

        public override void Dispose()
        {
            SystemBus.Unsubscribe<SquareShapeHitSignal>(OnSquareShapeHit);
            SystemBus.Unsubscribe<CircleShapeHitSignal>(OnCircleShapeHit);
            base.Dispose();
        }

        private void OnSquareShapeHit(SquareShapeHitSignal signal)
        {
        }

        private void OnCircleShapeHit(CircleShapeHitSignal signal)
        {
            
        }
    }
}