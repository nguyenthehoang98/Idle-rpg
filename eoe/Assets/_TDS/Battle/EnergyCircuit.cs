using System;

namespace _TDS.Battle
{
    public enum CircuitSlotContentType
    {
        Empty,
        Hero,
        Item,
    }

    public struct CircuitSlotContent
    {
        public CircuitSlotContentType Type { get; }
        public int Id { get; }

        private CircuitSlotContent(CircuitSlotContentType type, int id)
        {
            Type = type;
            Id = id;
        }

        public static CircuitSlotContent Empty => new CircuitSlotContent(CircuitSlotContentType.Empty, 0);

        public static CircuitSlotContent Hero(int id)
        {
            return Create(CircuitSlotContentType.Hero, id);
        }

        public static CircuitSlotContent Item(int id)
        {
            return Create(CircuitSlotContentType.Item, id);
        }

        private static CircuitSlotContent Create(CircuitSlotContentType type, int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Content id must be positive.");
            }

            return new CircuitSlotContent(type, id);
        }
    }

    public struct CircuitSlotState
    {
        public CircuitSlotContent Content { get; }
        public int Stack { get; }
        public float ActiveRemaining { get; }
        public bool IsActive => ActiveRemaining > 0f;

        internal CircuitSlotState(CircuitSlotContent content, int stack, float activeRemaining)
        {
            Content = content;
            Stack = stack;
            ActiveRemaining = activeRemaining;
        }
    }

    public struct CircuitActivationEvent
    {
        public int SlotIndex { get; }
        public CircuitSlotContent Content { get; }

        public CircuitActivationEvent(int slotIndex, CircuitSlotContent content)
        {
            SlotIndex = slotIndex;
            Content = content;
        }
    }

    public sealed class EnergyCircuit
    {
        public const int DefaultSlotCount = 8;
        public const float DefaultPulseInterval = 0.5f;
        public const int DefaultActivationThreshold = 3;
        public const float DefaultOverdriveDuration = 5f;

        private readonly CircuitSlotState[] slots;

        public int SlotCount => slots.Length;
        public float PulseInterval { get; }
        public int ActivationThreshold { get; }
        public float OverdriveDuration { get; }
        public int PulseIndex { get; private set; }

        public EnergyCircuit(
            int slotCount = DefaultSlotCount,
            float pulseInterval = DefaultPulseInterval,
            int activationThreshold = DefaultActivationThreshold,
            float overdriveDuration = DefaultOverdriveDuration)
        {
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount));
            if (pulseInterval <= 0f) throw new ArgumentOutOfRangeException(nameof(pulseInterval));
            if (activationThreshold <= 0) throw new ArgumentOutOfRangeException(nameof(activationThreshold));
            if (overdriveDuration <= 0f) throw new ArgumentOutOfRangeException(nameof(overdriveDuration));

            slots = new CircuitSlotState[slotCount];
            PulseInterval = pulseInterval;
            ActivationThreshold = activationThreshold;
            OverdriveDuration = overdriveDuration;
            Reset();
        }

        public CircuitSlotState GetSlot(int index)
        {
            ValidateSlotIndex(index);
            return slots[index];
        }

        public void SetContent(int index, CircuitSlotContent content)
        {
            ValidateSlotIndex(index);
            slots[index] = new CircuitSlotState(content, 0, 0f);
        }

        public void Reset()
        {
            PulseIndex = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new CircuitSlotState(CircuitSlotContent.Empty, 0, 0f);
            }
        }

        private void ValidateSlotIndex(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Slot index is outside the circuit.");
            }
        }
    }
}
