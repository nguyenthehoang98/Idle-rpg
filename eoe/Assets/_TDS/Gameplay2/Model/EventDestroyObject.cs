using _GameToolkit.ResourceManagement;
using UnityEngine;

namespace _TDS.Gameplay.Model
{
    public class EventDestroyObject : MonoBehaviour
    {
        public void Trigger()
        {
            Pool.Destroy(gameObject);
        }
    }
}