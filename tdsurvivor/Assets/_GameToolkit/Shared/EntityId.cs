using UnityEngine;

namespace _GameToolkit.Shared
{
    public class EntityId : MonoBehaviour
    {
        [SerializeField] private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}