using UnityEngine;

namespace _GameToolkit.Updater
{
    public abstract class BaseUpdatable : MonoBehaviour
    {
        public abstract void Tick(float deltaTime);
    }
}