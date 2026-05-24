using System;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.Utils;

namespace _Games.Battle.Logic
{
    public class Dice
    {
        public event Action<int> OnTriggerDice;
        
        private int value;
        private IDiceView view;
        private BattleSetting setting;
        private float elapsedTime;
        private Phase phase = Phase.Processing;

        public Dice(BattleShare share, BattleSetting setting)
        {
            this.setting = setting;
            this.elapsedTime = setting.slotCooldownTime;
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
                    value = RandomUtils.Range(1, 4);
                    elapsedTime = view.Roll(value);
                    phase = Phase.Rolling;
                    break;
                case Phase.Rolling:
                    // todo: post fx
                    elapsedTime = setting.slotRecoveryTime;
                    phase = Phase.Watting;
                    view.SetValue(value);
                    OnTriggerDice?.Invoke(value);
                    break;
                case Phase.Watting:
                    elapsedTime = setting.slotCooldownTime;
                    phase = Phase.Processing;
                    break;
            }
        }

        enum Phase
        {
            Processing, Rolling, Watting
        }
    }
}