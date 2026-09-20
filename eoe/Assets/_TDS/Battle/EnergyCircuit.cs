using System;

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
        public float ActiveRemaining { get; }
        public bool IsActive => ActiveRemaining > 0f;

        internal CircuitSlotState(CircuitSlotContent content, float activeRemaining)
        {
            Content = content;
            ActiveRemaining = activeRemaining;
        }
    }

    public struct CircuitActivationEvent
    {
        public int SlotIndex { get; }
        public CircuitSlotContent Content { get; }
        public int StackAtActivation { get; }
        public float PowerDuration { get; }

        public CircuitActivationEvent(int slotIndex, CircuitSlotContent content, int stackAtActivation)
            : this(slotIndex, content, stackAtActivation, EnergyCircuit.DefaultOverdriveDuration)
        {
        }

        public CircuitActivationEvent(
            int slotIndex,
            CircuitSlotContent content,
            int stackAtActivation,
            float powerDuration)
        {
            SlotIndex = slotIndex;
            Content = content;
            StackAtActivation = stackAtActivation;
            PowerDuration = powerDuration;
        }
    }

    public struct CircuitRollEvent
    {
        public int PreviousSlotIndex { get; }
        public int StepsMoved { get; }
        public int SlotIndex { get; }
        public CircuitSlotContent Content { get; }

        public CircuitRollEvent(
            int previousSlotIndex,
            int stepsMoved,
            int slotIndex,
            CircuitSlotContent content)
        {
            PreviousSlotIndex = previousSlotIndex;
            StepsMoved = stepsMoved;
            SlotIndex = slotIndex;
            Content = content;
        }
    }

    public sealed class EnergyCircuit
    {
        public const int DefaultSlotCount = 8;
        public const float DefaultEnergyCapacity = 100f;
        public const float DefaultPassiveEnergyPerSecond = 1f;
        public const float DefaultKillEnergy = 20f;
        public const float DefaultOverdriveDuration = 5f;

        private readonly CircuitSlotContent[] contents;
        private readonly CircuitItemType[] itemTypes;
        private readonly int[] itemPowers;
        private readonly float[] powerDurations;
        private readonly float defaultPowerDuration;

        private float activeRemaining;
        private int activeSlotIndex = -1;

        public int SlotCount => contents.Length;
        public float Energy { get; private set; }
        public float EnergyCapacity { get; }
        public float PassiveEnergyPerSecond { get; }
        public int HighlightIndex { get; private set; }
        public int PulseIndex => HighlightIndex;
        public bool IsReady => !IsPowerActive && Energy >= EnergyCapacity;
        public bool IsPowerActive => activeRemaining > 0f;
        public int ActiveSlotIndex => activeSlotIndex;

        public EnergyCircuit(
            int slotCount = DefaultSlotCount,
            float passiveEnergyPerSecond = DefaultPassiveEnergyPerSecond,
            float energyCapacity = DefaultEnergyCapacity,
            float overdriveDuration = DefaultOverdriveDuration)
        {
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount));
            if (passiveEnergyPerSecond < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(passiveEnergyPerSecond));
            }

            if (energyCapacity <= 0f) throw new ArgumentOutOfRangeException(nameof(energyCapacity));
            if (overdriveDuration <= 0f) throw new ArgumentOutOfRangeException(nameof(overdriveDuration));

            contents = new CircuitSlotContent[slotCount];
            itemTypes = new CircuitItemType[slotCount];
            itemPowers = new int[slotCount];
            powerDurations = new float[slotCount];
            EnergyCapacity = energyCapacity;
            PassiveEnergyPerSecond = passiveEnergyPerSecond;
            defaultPowerDuration = overdriveDuration;
            Reset();
        }

        public CircuitSlotState GetSlot(int index)
        {
            ValidateSlotIndex(index);
            return new CircuitSlotState(
                contents[index],
                index == activeSlotIndex ? activeRemaining : 0f);
        }

        public float GetPowerDuration(int index)
        {
            ValidateSlotIndex(index);
            return powerDurations[index];
        }

        public void SetPowerDuration(int index, float duration)
        {
            ValidateSlotIndex(index);
            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Power duration must be positive.");
            }

            powerDurations[index] = duration;
        }

        public void SetContent(int index, CircuitSlotContent content)
        {
            ValidateSlotIndex(index);
            itemTypes[index] = CircuitItemType.None;
            itemPowers[index] = 1;
            powerDurations[index] = defaultPowerDuration;
            contents[index] = content;
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
            powerDurations[index] = defaultPowerDuration;
            contents[index] = CircuitSlotContent.Item(id);
        }

        public CircuitItemType GetItemType(int index)
        {
            ValidateSlotIndex(index);
            return itemTypes[index];
        }

        public int GetItemPower(int index)
        {
            ValidateSlotIndex(index);
            return itemPowers[index];
        }

        public float AddEnergy(float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Energy amount cannot be negative.");
            }

            if (amount == 0f || IsPowerActive || IsReady)
            {
                return 0f;
            }

            float previous = Energy;
            Energy = Math.Min(EnergyCapacity, Energy + amount);
            return Energy - previous;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Circuit delta time cannot be negative.");
            }

            float remaining = deltaTime;
            if (IsPowerActive)
            {
                float consumed = Math.Min(remaining, activeRemaining);
                activeRemaining -= consumed;
                remaining -= consumed;

                if (activeRemaining <= 0f)
                {
                    activeRemaining = 0f;
                    activeSlotIndex = -1;
                }
            }

            if (remaining > 0f && !IsReady && !IsPowerActive)
            {
                AddEnergy(PassiveEnergyPerSecond * remaining);
            }
        }

        public bool TryRoll(
            int steps,
            out CircuitRollEvent roll,
            out CircuitActivationEvent activation)
        {
            ValidateSteps(steps);
            roll = default;
            activation = default;

            if (!IsReady)
            {
                return false;
            }

            int previousIndex = HighlightIndex;
            Energy = 0f;
            HighlightIndex = (HighlightIndex + steps) % SlotCount;
            CircuitSlotContent content = contents[HighlightIndex];
            roll = new CircuitRollEvent(previousIndex, steps, HighlightIndex, content);

            if (content.Type == CircuitSlotContentType.Empty)
            {
                return true;
            }

            activeSlotIndex = HighlightIndex;
            activeRemaining = powerDurations[HighlightIndex];
            activation = new CircuitActivationEvent(
                HighlightIndex,
                content,
                stackAtActivation: 0,
                powerDuration: powerDurations[HighlightIndex]);
            return true;
        }

        public void Reset()
        {
            Energy = 0f;
            HighlightIndex = 0;
            activeRemaining = 0f;
            activeSlotIndex = -1;

            for (int i = 0; i < contents.Length; i++)
            {
                contents[i] = CircuitSlotContent.Empty;
                itemTypes[i] = CircuitItemType.None;
                itemPowers[i] = 1;
                powerDurations[i] = defaultPowerDuration;
            }
        }

        private void ValidateSteps(int steps)
        {
            if (steps <= 0 || (SlotCount > 1 && steps >= SlotCount))
            {
                throw new ArgumentOutOfRangeException(nameof(steps), steps, "Roll steps must be within the circuit.");
            }
        }

        private void ValidateSlotIndex(int index)
        {
            if (index < 0 || index >= SlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Slot index is outside the circuit.");
            }
        }
    }
}
