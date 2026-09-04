using System;
using System.Collections.Generic;

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
        public int StackAtActivation { get; }

        public CircuitActivationEvent(int slotIndex, CircuitSlotContent content, int stackAtActivation)
        {
            SlotIndex = slotIndex;
            Content = content;
            StackAtActivation = stackAtActivation;
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
        // Index of the slot that will receive the next pulse.
        public int PulseIndex { get; private set; }

        private float elapsedSincePulse;

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

        public void Tick(float deltaTime, List<CircuitActivationEvent> activations)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (activations == null) throw new ArgumentNullException(nameof(activations));

            float remaining = deltaTime;
            while (remaining > 0f)
            {
                float timeToPulse = PulseInterval - elapsedSincePulse;
                float step = Math.Min(remaining, timeToPulse);
                AdvanceActiveSlots(step);
                elapsedSincePulse += step;
                remaining -= step;

                if (elapsedSincePulse < PulseInterval)
                {
                    continue;
                }

                elapsedSincePulse = 0f;
                ProcessPulse(activations);
                PulseIndex = (PulseIndex + 1) % SlotCount;
            }
        }

        private void AdvanceActiveSlots(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                CircuitSlotState slot = slots[i];
                if (!slot.IsActive)
                {
                    continue;
                }

                float activeRemaining = Math.Max(0f, slot.ActiveRemaining - deltaTime);
                slots[i] = new CircuitSlotState(slot.Content, slot.Stack, activeRemaining);
            }
        }

        private void ProcessPulse(List<CircuitActivationEvent> activations)
        {
            CircuitSlotState slot = slots[PulseIndex];
            if (slot.Content.Type == CircuitSlotContentType.Empty || slot.IsActive)
            {
                return;
            }

            int stack = slot.Stack + 1;
            if (stack < ActivationThreshold)
            {
                slots[PulseIndex] = new CircuitSlotState(slot.Content, stack, 0f);
                return;
            }

            activations.Add(new CircuitActivationEvent(PulseIndex, slot.Content, stack));
            slots[PulseIndex] = new CircuitSlotState(slot.Content, 0, OverdriveDuration);
        }

        public void Reset()
        {
            PulseIndex = 0;
            elapsedSincePulse = 0f;

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
