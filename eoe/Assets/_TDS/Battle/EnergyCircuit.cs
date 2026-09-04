using System;
using System.Collections.Generic;

namespace _TDS.Battle
{
    public enum CircuitItemType
    {
        None,
        Generator,
        Amplifier,
        Battery,
        Relay,
    }

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
        public int StoredEnergy { get; }
        public float ActiveRemaining { get; }
        public bool IsActive => ActiveRemaining > 0f;

        internal CircuitSlotState(
            CircuitSlotContent content,
            int stack,
            float activeRemaining,
            int storedEnergy = 0)
        {
            Content = content;
            Stack = stack;
            StoredEnergy = storedEnergy;
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
        private readonly CircuitItemType[] itemTypes;
        private readonly int[] itemPowers;

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
            itemTypes = new CircuitItemType[slotCount];
            itemPowers = new int[slotCount];
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
            itemTypes[index] = CircuitItemType.None;
            itemPowers[index] = 1;
            slots[index] = new CircuitSlotState(content, 0, 0f);
        }

        public void SetItem(int index, int id, CircuitItemType type, int power = 1)
        {
            ValidateSlotIndex(index);
            if (type == CircuitItemType.None)
            {
                throw new ArgumentException("An item slot needs an item type.", nameof(type));
            }

            if (power <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(power), power, "Item power must be positive.");
            }

            itemTypes[index] = type;
            itemPowers[index] = power;
            slots[index] = new CircuitSlotState(CircuitSlotContent.Item(id), 0, 0f);
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
                int stack = slot.Stack;
                int storedEnergy = slot.StoredEnergy;
                if (activeRemaining <= 0f && storedEnergy > 0)
                {
                    int restored = Math.Min(storedEnergy, ActivationThreshold - 1);
                    stack = restored;
                    storedEnergy -= restored;
                }

                slots[i] = new CircuitSlotState(slot.Content, stack, activeRemaining, storedEnergy);
            }
        }

        private void ProcessPulse(List<CircuitActivationEvent> activations)
        {
            CircuitSlotState beforePulse = slots[PulseIndex];
            if (beforePulse.Content.Type == CircuitSlotContentType.Empty || beforePulse.IsActive)
            {
                return;
            }

            ApplyEnergy(PulseIndex, 1, activations);
            CircuitSlotState afterPulse = slots[PulseIndex];
            if (afterPulse.IsActive)
            {
                return;
            }

            int nextIndex = (PulseIndex + 1) % SlotCount;
            switch (itemTypes[PulseIndex])
            {
                case CircuitItemType.Generator:
                    ApplyEnergy(nextIndex, itemPowers[PulseIndex], activations);
                    break;
                case CircuitItemType.Amplifier:
                    ApplyEnergy(nextIndex, itemPowers[PulseIndex] + 1, activations);
                    break;
                case CircuitItemType.Relay:
                    ApplyEnergy(nextIndex, beforePulse.Stack, activations);
                    break;
            }
        }

        private void ApplyEnergy(int index, int amount, List<CircuitActivationEvent> activations)
        {
            if (amount <= 0)
            {
                return;
            }

            CircuitSlotState slot = slots[index];
            if (slot.Content.Type == CircuitSlotContentType.Empty || slot.IsActive)
            {
                return;
            }

            int stack = slot.Stack + amount;
            if (stack < ActivationThreshold)
            {
                slots[index] = new CircuitSlotState(slot.Content, stack, 0f, slot.StoredEnergy);
                return;
            }

            int storedEnergy = slot.StoredEnergy;
            if (itemTypes[index] == CircuitItemType.Battery)
            {
                storedEnergy += stack - ActivationThreshold;
            }

            activations.Add(new CircuitActivationEvent(index, slot.Content, stack));
            slots[index] = new CircuitSlotState(slot.Content, 0, OverdriveDuration, storedEnergy);
        }

        public void Reset()
        {
            PulseIndex = 0;
            elapsedSincePulse = 0f;

            for (int i = 0; i < slots.Length; i++)
            {
                itemTypes[i] = CircuitItemType.None;
                itemPowers[i] = 1;
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
