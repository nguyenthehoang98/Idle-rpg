using _KIT.Event;
using UnityEngine;

namespace _Games.Combat.Events
{
    public readonly struct WaveSelectWeaponEvent : IEvent
    {
        public readonly float Duration;
        public readonly Vector3 CameraPosition;

        public WaveSelectWeaponEvent(float duration, Vector3 cameraPosition)
        {
            Duration = duration;
            CameraPosition = cameraPosition;
        }
    }
}