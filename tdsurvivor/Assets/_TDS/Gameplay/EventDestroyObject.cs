using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class EventDestroyObject : MonoBehaviour
    {
        public void Trigger()
        {
            Pool.Destroy(gameObject);
        }
    }
}