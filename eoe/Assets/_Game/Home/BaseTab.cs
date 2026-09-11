using UnityEngine;
using UnityEngine.Events;

namespace _Game.Home
{
    public class BaseTab : MonoBehaviour
    {
        [SerializeField] private UnityEvent onOpened = new UnityEvent();

        public UnityEvent OnOpened => onOpened;

        public virtual void Open()
        {
            onOpened.Invoke();
        }
    }
}
