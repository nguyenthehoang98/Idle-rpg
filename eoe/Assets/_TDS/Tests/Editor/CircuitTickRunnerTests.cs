using NUnit.Framework;
using UnityEngine;
using _TDS.Battle;
using _TDS.GameConfig;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public class CircuitTickRunnerTests
    {
        private GameObject gameObject;
        private CircuitTickRunner runner;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("CircuitTickRunnerTests");
            runner = gameObject.AddComponent<CircuitTickRunner>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void RunnerOwnsOneCircuitAndStartsAtZeroEnergy()
        {
            CircuitBoard board = CircuitBoard.FromHeroes(new[] { 1001 });

            runner.Initialize(board);

            Assert.That(runner.Circuit, Is.Not.Null);
            Assert.That(runner.Circuit.Energy, Is.EqualTo(0f));
            Assert.That(runner.Circuit.HighlightIndex, Is.EqualTo(0));
            Assert.That(runner.IsManualMode, Is.False);
        }

        [Test]
        public void KillEnergyTriggersAutoRollWhenEnergyReachesCapacity()
        {
            CircuitBoard board = CircuitBoard.FromHeroes(new[] { 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008 });
            runner.Initialize(board);
            int rollCount = 0;
            runner.OnRoll += _ => rollCount++;

            runner.AddKillEnergy(EnergyCircuit.DefaultEnergyCapacity);

            Assert.That(rollCount, Is.EqualTo(1));
            Assert.That(runner.Circuit.Energy, Is.EqualTo(0f));
            Assert.That(runner.Circuit.IsPowerActive, Is.True);
        }

        [Test]
        public void ManualModeHoldsReadyUntilRollIsRequested()
        {
            runner.Initialize(CircuitBoard.FromHeroes(new[] { 1001 }));
            runner.SetManualMode(true);
            runner.AddKillEnergy(EnergyCircuit.DefaultEnergyCapacity);

            Assert.That(runner.Circuit.IsReady, Is.True);
            Assert.That(runner.Circuit.HighlightIndex, Is.EqualTo(0));

            Assert.That(runner.TryRoll(1), Is.True);
            Assert.That(runner.Circuit.Energy, Is.EqualTo(0f));
            Assert.That(runner.Circuit.HighlightIndex, Is.EqualTo(1));
        }

        [Test]
        public void ManualModeDoesNotCreateTwoPowerActivations()
        {
            CircuitBoard board = new CircuitBoard();
            board.SetContent(1, CircuitSlotContent.Hero(1001));
            runner.Initialize(board);
            runner.SetManualMode(true);
            int activationCount = 0;
            runner.OnActivation += _ => activationCount++;

            runner.AddKillEnergy(EnergyCircuit.DefaultEnergyCapacity);
            Assert.That(runner.TryRoll(1), Is.True);
            runner.AddKillEnergy(EnergyCircuit.DefaultEnergyCapacity);
            Assert.That(runner.TryRoll(1), Is.False);

            Assert.That(activationCount, Is.EqualTo(1));
        }

        [Test]
        public void PassiveEnergyStartsAfterPowerEnds()
        {
            CircuitBoard board = new CircuitBoard();
            board.SetContent(1, CircuitSlotContent.Hero(1001));
            runner.Initialize(board);
            runner.SetManualMode(true);
            runner.AddKillEnergy(EnergyCircuit.DefaultEnergyCapacity);
            runner.TryRoll(1);

            runner.Tick(EnergyCircuit.DefaultOverdriveDuration);

            Assert.That(runner.Circuit.IsPowerActive, Is.False);
            Assert.That(runner.Circuit.Energy, Is.EqualTo(0f));
            runner.Tick(1f);
            Assert.That(runner.Circuit.Energy, Is.EqualTo(EnergyCircuit.DefaultPassiveEnergyPerSecond));
        }

        [Test]
        public void RunnerReportsRollEvenWhenLandingOnEmptySlot()
        {
            runner.Initialize(new CircuitBoard());
            runner.SetManualMode(true);
            runner.AddKillEnergy(EnergyCircuit.DefaultEnergyCapacity);
            CircuitRollEvent received = default;
            runner.OnRoll += roll => received = roll;

            Assert.That(runner.TryRoll(2), Is.True);

            Assert.That(received.StepsMoved, Is.EqualTo(2));
            Assert.That(received.SlotIndex, Is.EqualTo(2));
            Assert.That(runner.Circuit.IsPowerActive, Is.False);
        }
    }
}
