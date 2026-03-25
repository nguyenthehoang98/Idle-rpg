using _KIT.Event;
using UnityEngine;

namespace _Games.Combat.Event
{
    public struct WaveSelectWeaponEvent : IEvent
    {
        public float Duration;
        public Vector3 CameraPosition;

        public WaveSelectWeaponEvent(float duration, Vector3 cameraPosition)
        {
            Duration = duration;
            CameraPosition = cameraPosition;
        }
    }
}