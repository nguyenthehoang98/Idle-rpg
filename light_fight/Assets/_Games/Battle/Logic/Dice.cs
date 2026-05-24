using System;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.Utils;

namespace _Games.Battle.Logic
{
    public class Dice
    {
        public event Action<int, float> OnTriggerDice;
        
        private int value;
        private IDiceView view;
        private BattleSetting setting;
        private float elapsedTime;
        private Phase phase = Phase.Processing;

        public Dice(BattleShare share, BattleSetting setting)
        {
            this.setting = setting;
            this.elapsedTime = setting.diceCooldown;
            this.view = setting.dice.Instantiate();
            share.dices.Add(view);
        }

        public void Tick(float dt)
        {
            elapsedTime -= dt;
            
            if (elapsedTime > 0) return;
            
            switch (phase)
            {
                case Phase.Processing:
                    value = RandomUtils.Range(1, 7);
                    elapsedTime = view.Roll(value);
                    phase = Phase.Rolling;
                    break;
                case Phase.Rolling:
                    // todo: post fx
                    elapsedTime = setting.diceDelayTrigger;
                    phase = Phase.Delaying;
                    view.SetValue(value);
                    OnTriggerDice?.Invoke(value, setting.diceDelayTrigger);
                    break;
                case Phase.Delaying:
                    elapsedTime = setting.diceCooldown;
                    phase = Phase.Processing;
                    break;
            }
        }

        enum Phase
        {
            Processing, Rolling, Delaying,
        }
    }
}