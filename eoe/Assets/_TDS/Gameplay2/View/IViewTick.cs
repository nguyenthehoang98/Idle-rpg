using System.Threading.Tasks;

namespace _TDS.Gameplay.View
{
    public interface IViewTick
    {
        Task Initialize();
        void Tick(float deltaTime);
    }
}