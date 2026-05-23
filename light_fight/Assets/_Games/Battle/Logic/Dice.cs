using System;
using _Games.Battle.Model;
using _KITSystem.Utils;

namespace _Games.Battle.Logic
{
    public class Dice
    {
        public event Action<int> OnTriggerDice;
        
        public int Value { get; private set; }

        private readonly float cooldown;
        private readonly float delayTrigger;
        private float elapsedTime;
        private bool phaseRandom = true;
        private bool phaseWait;

        public Dice(BattleShare share, float cooldown, float delayTrigger)
        {
            this.cooldown = cooldown;
            this.delayTrigger = delayTrigger;
        }

        public void Tick(float dt)
        {
            if (phaseRandom)
            {
                elapsedTime += dt;
                if (elapsedTime >= cooldown)
                {
                    phaseRandom = false;
                    elapsedTime = 0; 
                    phaseWait = true;
                }
            }

            if (phaseWait)
            {
                elapsedTime += dt;
                if (elapsedTime >= delayTrigger)
                {
                    phaseWait = false;
                    Value = RandomUtils.Range(1, 7);
                    OnTriggerDice?.Invoke(Value);
                    elapsedTime = 0;
                    phaseRandom = true;
                }
            }
        }
    }
}