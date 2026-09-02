using _GameToolkit.Skills;
using _GameToolkit.Updater;

namespace _TDS.Gameplay.Manager
{
    public class SkillTickRunner : TickRunner
    {
        private SkillProcessingUnit processingUnits;

        private void Awake()
        {
            processingUnits = new SkillProcessingUnit();
        }

        public override void Tick(float deltaTime)
        {
            processingUnits.Tick(deltaTime);
        }
    }
}