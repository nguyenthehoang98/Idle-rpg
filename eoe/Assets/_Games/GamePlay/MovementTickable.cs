using System.Threading.Tasks;
using _KITSystem.Movement;
using _KITSystem.Schedule;

namespace _Games.GamePlay
{
    [System.Serializable]
    public class MovementTickable : MPU, ITickable
    {
        public new Task Initialize()
        {
            base.Initialize();
            return Task.CompletedTask;
        }
    }
}
