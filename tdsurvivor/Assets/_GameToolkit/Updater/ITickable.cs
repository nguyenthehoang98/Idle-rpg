using System.Threading.Tasks;

namespace _KITSystem.Schedule
{
    public interface ITickable
    {
        Task Initialize();
        void Tick(float deltaTime);
        void Dispose();
    }
}