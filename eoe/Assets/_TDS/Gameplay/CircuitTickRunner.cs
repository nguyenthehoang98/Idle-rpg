using System;
using _GameToolkit.GameConfig;
using _GameToolkit.Updater;
using _TDS.Battle;
using _TDS.GameConfig;
using UnityEngine;
using _TDS.Utils;

namespace _TDS.Gameplay
{
    public sealed class CircuitTickRunner : TickRunner
    {
        public EnergyCircuit Circuit { get; private set; }
        public int TickCount { get; private set; }
        public bool IsManualMode { get; private set; }
        public bool CanRoll => Circuit != null && Circuit.IsReady;
        public event Action<CircuitRollEvent> OnRoll;
        public event Action<CircuitActivationEvent> OnActivation;

        public void Initialize(CircuitBoard board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            Circuit = new EnergyCircuit(board.SlotCount);
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
                    && heroConfig.TryGetHero(content.Id, out HeroConfigData heroData)
                    && heroData.powerDuration > 0f)
                {
                    Circuit.SetPowerDuration(i, heroData.powerDuration);
                }
            }

            TickCount = 0;
            IsManualMode = false;
        }

        public override void Tick(float deltaTime)
        {
            if (Circuit == null)
            {
                return;
            }

            TickCount++;
            Circuit.Tick(deltaTime);

            foreach (Hero hero in Hero.AliveHeroes)
            {
                hero.TickOverdrive(deltaTime);
            }

            TryAutoRoll();
        }

        public void AddKillEnergy(float amount = EnergyCircuit.DefaultKillEnergy)
        {
            if (Circuit == null)
            {
                return;
            }

            Circuit.AddEnergy(amount);
            TryAutoRoll();
        }

        public void SetManualMode(bool manual)
        {
            IsManualMode = manual;
            TryAutoRoll();
        }

        public bool TryRoll()
        {
            if (Circuit == null || !Circuit.IsReady)
            {
                return false;
            }

            int steps = GameRng.Range(1, Math.Max(2, Circuit.SlotCount));
            return TryRoll(steps);
        }

        public bool TryRoll(int steps)
        {
            if (Circuit == null)
            {
                return false;
            }

            if (!Circuit.TryRoll(steps, out CircuitRollEvent roll, out CircuitActivationEvent activation))
            {
                return false;
            }

            OnRoll?.Invoke(roll);
            if (roll.Content.Type != CircuitSlotContentType.Empty)
            {
                OnActivation?.Invoke(activation);
            }

            return true;
        }

        public void ResetCircuit()
        {
            if (Circuit == null)
            {
                return;
            }

            Circuit.Reset();
            TickCount = 0;
        }

        private void TryAutoRoll()
        {
            if (IsManualMode || Circuit == null || !Circuit.IsReady)
            {
                return;
            }

            TryRoll();
        }
    }
}
