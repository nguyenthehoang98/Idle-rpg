using UnityEngine;

namespace _GameToolkit.Entity
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