using System;
using System.Collections.Generic;

namespace _TDS.Battle
{
    public sealed class CircuitBoard
    {
        public const int DefaultSlotCount = EnergyCircuit.DefaultSlotCount;

        private readonly CircuitSlotContent[] contents;

        public int SlotCount => contents.Length;

        public CircuitBoard(int slotCount = DefaultSlotCount)
        {
            if (slotCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCount));
            }

            contents = new CircuitSlotContent[slotCount];
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

        public CircuitSlotContent GetContent(int index)
        {
            ValidateSlotIndex(index);
            return contents[index];
        }

        public void SetContent(int index, CircuitSlotContent content)
        {
            ValidateSlotIndex(index);
            contents[index] = content;
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
