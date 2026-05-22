using System;
using System.Collections.Generic;
using _KITSystem.Utils;
using UnityEngine;

namespace _Games.Battle
{
    public class TestLogic
    {
        private Dice[] dices;
        private Cone[] cones;
        private int number;
        private HashSet<int> values = new HashSet<int>();

        public TestLogic(TestSetting setting)
        {
            this.dices = new Dice[setting.totalDice];
            this.cones = new Cone[BattleConst.MAX_DICE_NUMBER];
            
            for (int i = 0; i < setting.totalDice; i++)
            {
                dices[i] = new Dice(setting.diceCooldown, setting.diceDelayTrigger);
                dices[i].OnTriggerDice += TriggerDice;
            }
            
            for (int i = 0; i < cones.Length; i++)
            {
                cones[i] = new Cone(i, Vector3.zero, setting.weaponCooldown);
            }
        }

        private void TriggerDice(int dice)
        {
            number++;
            values.Add(dice - 1);
            if (number == dices.Length)
            {
                for (int i = 0; i < cones.Length; i++) cones[i].Inactive();
                for (int i = 0; i < cones.Length; i++)
                {
                    if (values.Contains(i))
                    {
                        cones[i].Active();
                    }
                }

                number = 0;
                values.Clear();
            }
        }

        public int[] DiceNumbers()
        {
            int[] numbers = new int[dices.Length];
            for (int i = 0; i < dices.Length; i++)
            {
                numbers[i] = dices[i].Value;
            }
            return numbers;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < dices.Length; i++)
            {
                dices[i].Tick(dt);
            }

            for (int i = 0; i < cones.Length; i++)
            {
                cones[i].Tick(dt);
            }
        }

        public void Draw()
        {
            for (int i = 0; i < cones.Length; i++)
            {
                cones[i].Draw();
            }
        }
    }

    [Serializable]
    public struct TestSetting
    {
        public int totalDice;
        public float diceCooldown;
        public float diceDelayTrigger;
        public float weaponCooldown;
    }
}