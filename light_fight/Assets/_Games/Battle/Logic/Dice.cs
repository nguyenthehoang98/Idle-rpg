using System;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.Utils;
using UnityEngine;

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
            SetSpeed(0);
        }

        public void Tick(float dt)
        {
            elapsedTime -= dt;

            if (phase == Phase.Processing)
            {
                float f = Mathf.Clamp01((setting.slotCooldownTime - elapsedTime) / setting.slotCooldownTime);
                view.SetProgress(f);
            }
            
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

        public void SetSpeed(float speed)
        {
            float f = Mathf.Clamp(speed, -1f, 1f);
            float v = Mathf.Lerp(setting.slotMinOffsetSpeed, setting.slotMaxOffsetSpeed, f);
            Color color = setting.slotColorDefaultSpeed;
            if (f > 0) color = Color.Lerp(color, setting.slotColorMaxSpeed, f);
            else if (f < 0) color = Color.Lerp(setting.slotColorMinSpeed, color, -f);
            view.SetColor(color);
        }

        enum Phase
        {
            Processing, Rolling, Watting
        }
    }
}