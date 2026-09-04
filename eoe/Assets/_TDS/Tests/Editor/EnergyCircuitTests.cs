using NUnit.Framework;
using _TDS.Battle;

namespace _TDS.Tests.Editor
{
    public class EnergyCircuitTests
    {
        [Test]
        public void NewCircuitUsesMvpDefaultsAndEmptySlots()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            Assert.That(circuit.SlotCount, Is.EqualTo(8));
            Assert.That(circuit.PulseIndex, Is.EqualTo(0));
            Assert.That(circuit.PulseInterval, Is.EqualTo(0.5f));
            Assert.That(circuit.ActivationThreshold, Is.EqualTo(3));
            Assert.That(circuit.OverdriveDuration, Is.EqualTo(5f));

            for (int i = 0; i < circuit.SlotCount; i++)
            {
                CircuitSlotState slot = circuit.GetSlot(i);
                Assert.That(slot.Content.Type, Is.EqualTo(CircuitSlotContentType.Empty));
                Assert.That(slot.Stack, Is.EqualTo(0));
                Assert.That(slot.IsActive, Is.False);
            }
        }

        [Test]
        public void SlotCanStoreHeroOrItemContent()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            circuit.SetContent(2, CircuitSlotContent.Hero(101));
            circuit.SetContent(5, CircuitSlotContent.Item(201));

            Assert.That(circuit.GetSlot(2).Content.Type, Is.EqualTo(CircuitSlotContentType.Hero));
            Assert.That(circuit.GetSlot(2).Content.Id, Is.EqualTo(101));
            Assert.That(circuit.GetSlot(5).Content.Type, Is.EqualTo(CircuitSlotContentType.Item));
            Assert.That(circuit.GetSlot(5).Content.Id, Is.EqualTo(201));
        }

        [Test]
        public void ActivationEventCarriesSlotAndContent()
        {
            CircuitActivationEvent activation = new CircuitActivationEvent(
                3,
                CircuitSlotContent.Hero(101));

            Assert.That(activation.SlotIndex, Is.EqualTo(3));
            Assert.That(activation.Content.Type, Is.EqualTo(CircuitSlotContentType.Hero));
            Assert.That(activation.Content.Id, Is.EqualTo(101));
        }

        [Test]
        public void SlotIndexMustBeWithinCircuit()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => circuit.GetSlot(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => circuit.SetContent(8, CircuitSlotContent.Item(1)));
        }
    }
}
