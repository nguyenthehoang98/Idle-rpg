using _KIT.Event;
using Unity.Mathematics;

namespace _Game.Battle.Ecs.Events
{
    public readonly struct OpenEquipmentSelectionEvent : IEvent
    {
        public readonly float2 CameraOffsetPosition;
        public readonly float OrthoSize;
        public readonly float Duration;

        public OpenEquipmentSelectionEvent(float duration, float orthoSize, float2 cameraOffsetPosition)
        {
            Duration = duration;
            OrthoSize = orthoSize;
            CameraOffsetPosition = cameraOffsetPosition;
        }
    }
}