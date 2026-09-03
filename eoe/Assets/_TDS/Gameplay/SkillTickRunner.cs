using _GameToolkit.Skills;
using _GameToolkit.Updater;

namespace _TDS.Gameplay
{
    public class SkillTickRunner : TickRunner
    {
        private SkillProcessingUnit processingUnits;

        public void Initialize()
        {
            processingUnits = new SkillProcessingUnit();
        }

        public SkillProcessingUnit Unit
        {
            get { return processingUnits; }
        }

        public override void Tick(float deltaTime)
        {
            processingUnits.Tick(deltaTime);
        }
    }
}