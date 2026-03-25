using _KIT.Event;
using UnityEngine;

namespace _Games.Combat.Event
{
    public struct WaveSelectWeaponEvent : IEvent
    {
        public float Duration;
        public float CameraOrtho;
        public Vector3 CameraPosition;

        public WaveSelectWeaponEvent(float duration, float cameraOrtho, Vector3 cameraPosition)
        {
            Duration = duration;
            CameraOrtho = cameraOrtho;
            CameraPosition = cameraPosition;
        }
    }
}