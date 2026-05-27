using System;
using _FightCode.Battle.Model;
using _FightCode.Battle.View;
using _KITSystem.Utils;
using UnityEngine;

namespace _FightCode.Battle.Logic
{
    public class Dice
    {
        public event Action<int> OnTriggerDice;

        private float speed;
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
            SetSpeed(1);
        }

        public void Tick(float dt)
        {
            elapsedTime -= dt * speed;

            if (phase == Phase.Processing)
            {
                float f = Mathf.Clamp01((setting.slotCooldownTime - elapsedTime) / setting.slotCooldownTime);
                view.SetProgress(f);
            }
            
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

        public void SetSpeed(float v)
        {
            speed = v;
            view.SetSpeed(v);
        }

        enum Phase
        {
            Processing, Rolling, Watting
        }
    }
}