using System.Collections.Generic;
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
                CircuitSlotContent.Hero(101),
                3);

            Assert.That(activation.SlotIndex, Is.EqualTo(3));
            Assert.That(activation.Content.Type, Is.EqualTo(CircuitSlotContentType.Hero));
            Assert.That(activation.Content.Id, Is.EqualTo(101));
            Assert.That(activation.StackAtActivation, Is.EqualTo(3));
        }

        [Test]
        public void PulseAddsStackOnlyToContentAtCurrentIndex()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(0, CircuitSlotContent.Hero(101));
            List<CircuitActivationEvent> activations = new List<CircuitActivationEvent>();

            circuit.Tick(circuit.PulseInterval, activations);

            Assert.That(circuit.GetSlot(0).Stack, Is.EqualTo(1));
            Assert.That(circuit.GetSlot(1).Stack, Is.EqualTo(0));
            Assert.That(activations, Is.Empty);
        }

        [Test]
        public void ThresholdCreatesOneActivationAndResetsStack()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(0, CircuitSlotContent.Hero(101));
            List<CircuitActivationEvent> activations = new List<CircuitActivationEvent>();

            circuit.Tick(8.5f, activations);

            Assert.That(activations, Has.Count.EqualTo(1));
            Assert.That(activations[0].SlotIndex, Is.EqualTo(0));
            Assert.That(activations[0].StackAtActivation, Is.EqualTo(3));
            Assert.That(circuit.GetSlot(0).Stack, Is.EqualTo(0));
            Assert.That(circuit.GetSlot(0).IsActive, Is.True);
        }

        [Test]
        public void ActiveSlotDoesNotAccumulateOrReactivate()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(0, CircuitSlotContent.Hero(101));
            List<CircuitActivationEvent> activations = new List<CircuitActivationEvent>();

            circuit.Tick(8.5f, activations);
            circuit.Tick(4f, activations);

            Assert.That(activations, Has.Count.EqualTo(1));
            Assert.That(circuit.GetSlot(0).Stack, Is.EqualTo(0));
        }

        [Test]
        public void PulseDoesNotAdvanceBeforeInterval()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            circuit.Tick(0.49f, new List<CircuitActivationEvent>());

            Assert.That(circuit.PulseIndex, Is.EqualTo(0));
        }

        [Test]
        public void PulseAdvancesOneSlotPerIntervalAndWraps()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            List<CircuitActivationEvent> activations = new List<CircuitActivationEvent>();

            for (int i = 0; i < circuit.SlotCount; i++)
            {
                circuit.Tick(circuit.PulseInterval, activations);
                Assert.That(circuit.PulseIndex, Is.EqualTo((i + 1) % circuit.SlotCount));
            }
        }

        [Test]
        public void LargeDeltaTimeProcessesEveryPulse()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            circuit.Tick(2.1f, new List<CircuitActivationEvent>());

            Assert.That(circuit.PulseIndex, Is.EqualTo(4));
        }

        [Test]
        public void EmptySlotsDoNotStopPulseTraversal()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            circuit.Tick(4f, new List<CircuitActivationEvent>());

            Assert.That(circuit.PulseIndex, Is.EqualTo(0));
        }

        [Test]
        public void CircuitSupportsMultipleContentSlots()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(0, CircuitSlotContent.Hero(101));
            circuit.SetContent(1, CircuitSlotContent.Item(201));
            List<CircuitActivationEvent> activations = new List<CircuitActivationEvent>();

            circuit.Tick(1f, activations);

            Assert.That(circuit.GetSlot(0).Stack, Is.EqualTo(1));
            Assert.That(circuit.GetSlot(1).Stack, Is.EqualTo(1));
        }

        [Test]
        public void CircuitCanRunThreeCompleteLoops()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            circuit.Tick(circuit.PulseInterval * circuit.SlotCount * 3, new List<CircuitActivationEvent>());

            Assert.That(circuit.PulseIndex, Is.EqualTo(0));
        }

        [Test]
        public void ResetReturnsCircuitToInitialState()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(0, CircuitSlotContent.Hero(101));
            circuit.Tick(8.5f, new List<CircuitActivationEvent>());

            circuit.Reset();

            Assert.That(circuit.PulseIndex, Is.EqualTo(0));
            Assert.That(circuit.GetSlot(0).Content.Type, Is.EqualTo(CircuitSlotContentType.Empty));
            Assert.That(circuit.GetSlot(0).Stack, Is.EqualTo(0));
            Assert.That(circuit.GetSlot(0).IsActive, Is.False);
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
