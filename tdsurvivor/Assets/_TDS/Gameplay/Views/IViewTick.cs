using System.Threading.Tasks;

namespace _Game.GamePlay.View
{
    public interface IViewTick
    {
        Task Initialize();
        void Tick(float deltaTime);
    }
}