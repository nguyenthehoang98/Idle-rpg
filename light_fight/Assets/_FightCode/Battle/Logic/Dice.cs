using System;
using _FightCode.Battle.Model;
using _FightCode.Battle.View;
using _KITSystem.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _FightCode.Battle.Logic
{
    public class Dice
    {
        public event Action<int> OnTriggerDice;

        private readonly DiceView view;
        private readonly float cooldown;
        private readonly float recoveryTime;
        
        private float speed;
        private int value;
        private Phase phase = Phase.Processing;
        
        private float elapsedTime;

        public Dice(BattleSetting setting, DiceView view)
        {
            this.view = view;
            elapsedTime = cooldown = setting.slotCooldownTime;
            recoveryTime = setting.slotRecoveryTime;
            SetSpeed(1);
        }

        public void Tick(float dt)
        {
            elapsedTime -= dt * speed;

            if (phase == Phase.Processing)
            {
                float f = Mathf.Clamp01((cooldown - elapsedTime) / cooldown);
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
                    elapsedTime = recoveryTime;
                    phase = Phase.Watting;
                    view.SetValue(value);
                    OnTriggerDice?.Invoke(value);
                    break;
                case Phase.Watting:
                    elapsedTime = cooldown;
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