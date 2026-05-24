using System;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.Utils;

namespace _Games.Battle.Logic
{
    public class Dice
    {
        public event Action<int> OnTriggerDice;
        
        public int Value { get; private set; }

        private BattleSetting setting;
        private float elapsedTime;
        private bool phaseRandom = true;
        private bool phaseWait;

        public Dice(BattleShare share, BattleSetting setting)
        {
            this.setting = setting;
        }

        public void Tick(float dt)
        {
            if (phaseRandom)
            {
                elapsedTime += dt;
                if (elapsedTime >= setting.diceCooldown)
                {
                    phaseRandom = false;
                    elapsedTime = 0; 
                    phaseWait = true;
                }
            }

            if (phaseWait)
            {
                elapsedTime += dt;
                if (elapsedTime >= setting.diceDelayTrigger)
                {
                    phaseWait = false;
                    Value = RandomUtils.Range(1, 3);
                    OnTriggerDice?.Invoke(Value);
                    elapsedTime = 0;
                    phaseRandom = true;
                }
            }
        }
    }
}