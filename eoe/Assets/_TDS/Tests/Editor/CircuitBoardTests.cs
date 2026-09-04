using NUnit.Framework;
using _TDS.Battle;

namespace _TDS.Tests.Editor
{
    public class CircuitBoardTests
    {
        [Test]
        public void NewBoardHasEightEmptySlots()
        {
            CircuitBoard board = new CircuitBoard();

            Assert.That(board.SlotCount, Is.EqualTo(8));
            for (int i = 0; i < board.SlotCount; i++)
            {
                Assert.That(board.GetContent(i).Type, Is.EqualTo(CircuitSlotContentType.Empty));
            }
        }

        [Test]
        public void BoardMapsConfiguredHeroesAndKeepsEmptySlots()
        {
            CircuitBoard board = CircuitBoard.FromHeroes(new[] { 101, 0, 102 });

            Assert.That(board.GetContent(0).Type, Is.EqualTo(CircuitSlotContentType.Hero));
            Assert.That(board.GetContent(0).Id, Is.EqualTo(101));
            Assert.That(board.GetContent(1).Type, Is.EqualTo(CircuitSlotContentType.Empty));
            Assert.That(board.GetContent(2).Type, Is.EqualTo(CircuitSlotContentType.Hero));
            Assert.That(board.GetContent(2).Id, Is.EqualTo(102));
        }

        [Test]
        public void BoardCanReplaceAndClearSlotContent()
        {
            CircuitBoard board = new CircuitBoard();

            board.SetContent(3, CircuitSlotContent.Item(201));
            board.ClearSlot(3);

            Assert.That(board.GetContent(3).Type, Is.EqualTo(CircuitSlotContentType.Empty));
        }

        [Test]
        public void BoardRejectsInvalidSlotAndContentIds()
        {
            CircuitBoard board = new CircuitBoard();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.GetContent(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.SetContent(8, CircuitSlotContent.Hero(101)));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.SetContent(0, CircuitSlotContent.Hero(0)));
        }
    }
}
