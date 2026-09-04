using System;
using System.Collections.Generic;
using _GameToolkit.Updater;
using _TDS.Battle;

namespace _TDS.Gameplay
{
    public sealed class CircuitTickRunner : TickRunner
    {
        private readonly List<CircuitActivationEvent> activations = new List<CircuitActivationEvent>();

        public EnergyCircuit Circuit { get; private set; }
        public int TickCount { get; private set; }
        public event Action<CircuitActivationEvent> OnActivation;

        public void Initialize(CircuitBoard board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            Circuit = new EnergyCircuit(board.SlotCount);
            for (int i = 0; i < board.SlotCount; i++)
            {
                Circuit.SetContent(i, board.GetContent(i));
            }

            TickCount = 0;
            activations.Clear();
        }

        public override void Tick(float deltaTime)
        {
            if (Circuit == null)
            {
                return;
            }

            TickCount++;
            activations.Clear();
            Circuit.Tick(deltaTime, activations);

            for (int i = 0; i < activations.Count; i++)
            {
                OnActivation?.Invoke(activations[i]);
            }

            foreach (Hero hero in Hero.AliveHeroes)
            {
                hero.TickOverdrive(deltaTime);
            }
        }

        public void ResetCircuit()
        {
            if (Circuit == null)
            {
                return;
            }

            Circuit.Reset();
            TickCount = 0;
            activations.Clear();
        }
    }
}
