using System;
using UnityEngine;

namespace _TDS.Gameplay
{
    public class HeroSlot : MonoBehaviour
    {
        [SerializeField] private Animator arrow;
        [SerializeField] private Animator add;

        public void Activate()
        {
            arrow.gameObject.SetActive(true);
            arrow.Play("Play", 0, 0f);
            
            add.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            arrow.gameObject.SetActive(false);
            add.gameObject.SetActive(false);
        }
    }
}
