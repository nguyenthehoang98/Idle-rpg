using System;
using NUnit.Framework;
using _TDS.Battle;

namespace _TDS.Tests.Editor
{
    public class EnergyCircuitTests
    {
        [Test]
        public void NewCircuitStartsEmptyAndNotReady()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            Assert.That(circuit.SlotCount, Is.EqualTo(8));
            Assert.That(circuit.Energy, Is.EqualTo(0f));
            Assert.That(circuit.EnergyCapacity, Is.EqualTo(100f));
            Assert.That(circuit.HighlightIndex, Is.EqualTo(0));
            Assert.That(circuit.IsReady, Is.False);
            Assert.That(circuit.IsPowerActive, Is.False);
        }

        [Test]
        public void PassiveEnergyAccumulatesOnlyWhileCharging()
        {
            EnergyCircuit circuit = new EnergyCircuit(passiveEnergyPerSecond: 10f);

            circuit.Tick(3f);

            Assert.That(circuit.Energy, Is.EqualTo(30f));
            Assert.That(circuit.HighlightIndex, Is.EqualTo(0));
        }

        [Test]
        public void KillEnergyIsClampedAndReadyDoesNotAccumulate()
        {
            EnergyCircuit circuit = new EnergyCircuit(passiveEnergyPerSecond: 10f);

            circuit.AddEnergy(90f);
            circuit.AddEnergy(30f);
            circuit.Tick(5f);

            Assert.That(circuit.Energy, Is.EqualTo(100f));
            Assert.That(circuit.IsReady, Is.True);
        }

        [Test]
        public void RollRequiresFullEnergy()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            Assert.That(circuit.TryRoll(1, out _, out _), Is.False);
            circuit.AddEnergy(100f);
            Assert.That(circuit.TryRoll(1, out _, out _), Is.True);
        }

        [Test]
        public void RollMovesHighlightByRequestedStepsAndReportsDestination()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(3, CircuitSlotContent.Hero(101));
            circuit.AddEnergy(100f);

            Assert.That(circuit.TryRoll(3, out CircuitRollEvent roll, out CircuitActivationEvent activation), Is.True);

            Assert.That(roll.PreviousSlotIndex, Is.EqualTo(0));
            Assert.That(roll.StepsMoved, Is.EqualTo(3));
            Assert.That(roll.SlotIndex, Is.EqualTo(3));
            Assert.That(roll.Content.Type, Is.EqualTo(CircuitSlotContentType.Hero));
            Assert.That(activation.SlotIndex, Is.EqualTo(3));
            Assert.That(circuit.HighlightIndex, Is.EqualTo(3));
            Assert.That(circuit.Energy, Is.EqualTo(0f));
        }

        [Test]
        public void RollUsesConfiguredDefaultPowerDuration()
        {
            EnergyCircuit circuit = new EnergyCircuit(overdriveDuration: 10f);
            circuit.SetContent(1, CircuitSlotContent.Hero(101));
            circuit.AddEnergy(100f);

            Assert.That(circuit.TryRoll(1, out _, out CircuitActivationEvent activation), Is.True);

            Assert.That(activation.PowerDuration, Is.EqualTo(10f));
            Assert.That(circuit.GetSlot(1).ActiveRemaining, Is.EqualTo(10f));
        }

        [Test]
        public void EmptyLandingDoesNotActivateAndCanRecharge()
        {
            EnergyCircuit circuit = new EnergyCircuit(passiveEnergyPerSecond: 10f);
            circuit.AddEnergy(100f);

            Assert.That(circuit.TryRoll(1, out CircuitRollEvent roll, out _), Is.True);
            Assert.That(roll.Content.Type, Is.EqualTo(CircuitSlotContentType.Empty));
            Assert.That(circuit.IsPowerActive, Is.False);

            circuit.Tick(2f);

            Assert.That(circuit.Energy, Is.EqualTo(20f));
        }

        [Test]
        public void NonEmptyLandingStartsOnePowerAndBlocksSecondRoll()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(1, CircuitSlotContent.Hero(101));
            circuit.AddEnergy(100f);

            Assert.That(circuit.TryRoll(1, out _, out CircuitActivationEvent activation), Is.True);
            Assert.That(activation.Content.Type, Is.EqualTo(CircuitSlotContentType.Hero));
            Assert.That(circuit.IsPowerActive, Is.True);
            Assert.That(circuit.ActiveSlotIndex, Is.EqualTo(1));
            Assert.That(circuit.TryRoll(1, out _, out _), Is.False);
        }

        [Test]
        public void EnergyStartsAgainOnlyAfterPowerEnds()
        {
            EnergyCircuit circuit = new EnergyCircuit(passiveEnergyPerSecond: 10f);
            circuit.SetContent(1, CircuitSlotContent.Hero(101));
            circuit.AddEnergy(100f);
            circuit.TryRoll(1, out _, out _);

            circuit.Tick(4f);
            Assert.That(circuit.Energy, Is.EqualTo(0f));
            Assert.That(circuit.IsPowerActive, Is.True);

            circuit.Tick(1f);
            Assert.That(circuit.IsPowerActive, Is.False);
            Assert.That(circuit.Energy, Is.EqualTo(0f));

            circuit.Tick(1f);
            Assert.That(circuit.Energy, Is.EqualTo(10f));
        }

        [Test]
        public void RollWrapsHighlightAroundTheCircuit()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.AddEnergy(100f);

            Assert.That(circuit.TryRoll(7, out _, out _), Is.True);
            circuit.Tick(1f);
            circuit.AddEnergy(100f);
            Assert.That(circuit.TryRoll(3, out CircuitRollEvent roll, out _), Is.True);

            Assert.That(roll.SlotIndex, Is.EqualTo(2));
            Assert.That(circuit.HighlightIndex, Is.EqualTo(2));
        }

        [Test]
        public void InvalidStepIsRejected()
        {
            EnergyCircuit circuit = new EnergyCircuit();

            Assert.Throws<ArgumentOutOfRangeException>(() => circuit.TryRoll(0, out _, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => circuit.TryRoll(circuit.SlotCount, out _, out _));
        }

        [Test]
        public void ResetClearsEnergyHighlightAndPower()
        {
            EnergyCircuit circuit = new EnergyCircuit();
            circuit.SetContent(1, CircuitSlotContent.Hero(101));
            circuit.AddEnergy(100f);
            circuit.TryRoll(1, out _, out _);

            circuit.Reset();

            Assert.That(circuit.Energy, Is.EqualTo(0f));
            Assert.That(circuit.HighlightIndex, Is.EqualTo(0));
            Assert.That(circuit.IsPowerActive, Is.False);
            Assert.That(circuit.GetSlot(1).Content.Type, Is.EqualTo(CircuitSlotContentType.Empty));
        }
    }
}
