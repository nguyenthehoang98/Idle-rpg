using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class EventDestroyObject : MonoBehaviour
    {
        public void Destroy()
        {
            Pool.Destroy(gameObject);
        }
    }
}