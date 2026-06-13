using System.Threading.Tasks;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Core;

namespace _Games.GamePlay.SkillSystem
{
    [System.Serializable]
    public class SkillTickable : Spu, ITickable
    {
        public Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}
