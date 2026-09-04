using NUnit.Framework;
using UnityEngine;
using _TDS.Battle;
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
        public void RunnerOwnsOneCircuitAndEmitsActivationOnce()
        {
            CircuitBoard board = CircuitBoard.FromHeroes(new[] { 101 });
            int activationCount = 0;
            runner.OnActivation += _ => activationCount++;

            runner.Initialize(board);
            runner.Tick(4.01f);
            runner.Tick(4.01f);
            runner.Tick(4.01f);

            Assert.That(runner.TickCount, Is.EqualTo(3));
            Assert.That(runner.Circuit, Is.Not.Null);
            Assert.That(activationCount, Is.EqualTo(1));
        }

        [Test]
        public void ResetRestartsCircuitAndTickCount()
        {
            runner.Initialize(CircuitBoard.FromHeroes(new[] { 101 }));
            runner.Tick(0.5f);
            runner.ResetCircuit();

            Assert.That(runner.TickCount, Is.EqualTo(0));
            Assert.That(runner.Circuit.PulseIndex, Is.EqualTo(0));
            Assert.That(runner.Circuit.GetSlot(0).Stack, Is.EqualTo(0));
        }
    }
}
