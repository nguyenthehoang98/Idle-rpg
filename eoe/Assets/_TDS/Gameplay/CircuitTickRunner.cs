using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using _GameToolkit.Updater;
using _TDS.Battle;
using _TDS.GameConfig;

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
            // ponytail: try/catch vì test/editor có thể Initialize khi chưa load config.
            HeroConfig heroConfig = null;
            try { heroConfig = ConfigManager.Get<HeroConfig>(); } catch { heroConfig = null; }
            for (int i = 0; i < board.SlotCount; i++)
            {
                CircuitSlotContent content = board.GetContent(i);
                if (content.Type == CircuitSlotContentType.Item)
                {
                    Circuit.SetItem(i, content.Id, board.GetItemType(i), board.GetItemPower(i));
                }
                else
                {
                    Circuit.SetContent(i, content);
                }

                Circuit.SetPowerDuration(i, board.GetPowerDuration(i));

                if (content.Type == CircuitSlotContentType.Hero
                    && heroConfig != null
                    && heroConfig.TryGetHero(content.Id, out HeroConfigData heroData))
                {
                    if (heroData.stackThreshold > 0)
                    {
                        Circuit.SetThreshold(i, heroData.stackThreshold);
                    }

                    if (heroData.powerDuration > 0f)
                    {
                        Circuit.SetPowerDuration(i, heroData.powerDuration);
                    }
                }
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
