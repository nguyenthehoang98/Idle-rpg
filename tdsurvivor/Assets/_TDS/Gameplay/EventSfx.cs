using _Game.GamePlay.Manager;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class EventSfx : MonoBehaviour
    {
        [SerializeField] private AudioClip sfx;

        public void Trigger()
        {
            SoundManager.Instance.PlayOneShot(sfx);
        }
    }
}