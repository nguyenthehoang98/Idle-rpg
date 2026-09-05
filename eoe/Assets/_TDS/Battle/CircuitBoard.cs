using System;
using System.Collections.Generic;

namespace _TDS.Battle
{
    public sealed class CircuitBoard
    {
        public const int DefaultSlotCount = EnergyCircuit.DefaultSlotCount;

        private readonly CircuitSlotContent[] contents;
        private readonly CircuitItemType[] itemTypes;
        private readonly int[] itemPowers;

        public int SlotCount => contents.Length;

        public CircuitBoard(int slotCount = DefaultSlotCount)
        {
            if (slotCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCount));
            }

            contents = new CircuitSlotContent[slotCount];
            itemTypes = new CircuitItemType[slotCount];
            itemPowers = new int[slotCount];
            Reset();
        }

        public static CircuitBoard FromHeroes(IReadOnlyList<int> heroIds)
        {
            if (heroIds == null)
            {
                throw new ArgumentNullException(nameof(heroIds));
            }

            CircuitBoard board = new CircuitBoard();
            int count = Math.Min(heroIds.Count, board.SlotCount);
            for (int i = 0; i < count; i++)
            {
                if (heroIds[i] == 0)
                {
                    continue;
                }

                board.SetContent(i, CircuitSlotContent.Hero(heroIds[i]));
            }

            return board;
        }

        public static CircuitBoard FromHeroesWithStarterGenerator(IReadOnlyList<int> heroIds)
        {
            if (heroIds == null)
            {
                throw new ArgumentNullException(nameof(heroIds));
            }

            CircuitBoard board = new CircuitBoard();
            board.SetItem(0, 201, CircuitItemType.Generator);

            int count = Math.Min(heroIds.Count, board.SlotCount - 1);
            for (int i = 0; i < count; i++)
            {
                if (heroIds[i] == 0)
                {
                    continue;
                }

                board.SetContent(i + 1, CircuitSlotContent.Hero(heroIds[i]));
            }

            return board;
        }

        public CircuitSlotContent GetContent(int index)
        {
            ValidateSlotIndex(index);
            return contents[index];
        }

        public void SetContent(int index, CircuitSlotContent content)
        {
            ValidateSlotIndex(index);
            contents[index] = content;
            itemTypes[index] = CircuitItemType.None;
            itemPowers[index] = 1;
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

            contents[index] = CircuitSlotContent.Item(id);
            itemTypes[index] = type;
            itemPowers[index] = power;
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

        public void ClearSlot(int index)
        {
            SetContent(index, CircuitSlotContent.Empty);
        }

        public void Reset()
        {
            for (int i = 0; i < contents.Length; i++)
            {
                contents[i] = CircuitSlotContent.Empty;
                itemTypes[i] = CircuitItemType.None;
                itemPowers[i] = 1;
            }
        }

        private void ValidateSlotIndex(int index)
        {
            if (index < 0 || index >= contents.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Slot index is outside the board.");
            }
        }
    }
}
