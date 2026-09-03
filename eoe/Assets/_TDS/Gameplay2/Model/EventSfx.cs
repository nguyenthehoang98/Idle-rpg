using _GameToolkit.Share;
using UnityEngine;

namespace _TDS.Gameplay.Model
{
    public class EventSfx : MonoBehaviour
    {
        [SerializeField] private AudioClip sfx;

        public void Trigger()
        {
            SoundUtils.Instance.PlayOneShot(sfx);
        }
    }
}