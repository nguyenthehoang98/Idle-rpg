using _GameToolkit.ResourceManagement;
using UnityEngine;

namespace _TDS.Utils
{
    public class PoolObject : MonoBehaviour
    {
        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}