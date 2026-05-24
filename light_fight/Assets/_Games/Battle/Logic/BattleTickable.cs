using System;
using System.Collections;
using System.Collections.Generic;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle.Logic
{
    [Serializable]
    public class BattleTickable : ITickable
    {
        public event Action OnInitialized;
        
        private bool isInitialized = false;
        private Dice[] dices;
        private Slot[] slots;
        private int totalDiceActivate;
        private int[] diceNumberStacks = new int[BattleConst.MAX_DICE_NUMBER];
        private Vector3 center;
        private float attackRange;

        public void Initialize(BattleShare share, BattleSetting setting, IQuery query)
        {
            this.attackRange = setting.weaponAttackRange;
            this.center = new Vector3(setting.center.x, setting.center.y);
            this.dices = new Dice[setting.totalDice];
            this.slots = new Slot[BattleConst.MAX_DICE_NUMBER];

            for (int i = 0; i < setting.totalDice; i++)
            {
                dices[i] = new Dice(share, setting);
                dices[i].OnTriggerDice += TriggerDice;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new Slot(share, i, setting, query);
            }

            CoroutineUtils.Run(share.owner, InitCoroutine(share.timeScale));
        }
        
        private IEnumerator InitCoroutine(float timeScale)
        {
            yield return new WaitForSeconds(1f / timeScale);

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Initialize();
                yield return new WaitForSeconds(0.075f / timeScale);
            }

            yield return new WaitForSeconds(0.1f / timeScale);

            float play = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                play = slots[i].Play();
            }

            yield return new WaitForSeconds((play) / timeScale);

            isInitialized = true;

            OnInitialized?.Invoke();
        }

        private void TriggerDice(int dice)
        {
            if (!isInitialized)
            {
#if UNITY_EDITOR
                Debug.LogError("BattleTickable not initialized");
#endif
                return;
            }

            totalDiceActivate++;
            diceNumberStacks[dice - 1]++;
            if (totalDiceActivate == dices.Length)
            {
                int count = BattleConst.MAX_DICE_NUMBER;
                for (int i = 0; i < count; i++)
                {
                    int stack = diceNumberStacks[i];
                    Slot slot = slots[i];
                    
                    if (stack == 0)
                    {
                        if(slot.IsPlaying) slots[i].Deactivate();
                    }
                    else
                    {
                        if (!slot.IsPlaying) slots[i].Activate();
                        slots[i].DoStack(stack);
                    }
                }

                Array.Clear(diceNumberStacks, 0, diceNumberStacks.Length);
                totalDiceActivate = 0;
            }
        }

        public void Tick(float dt)
        {
            if (!isInitialized) return;

            for (int i = 0; i < dices.Length; i++)
            {
                dices[i].Tick(dt);
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Tick(dt);
            }
        }

        public void Draw()
        {
            if (!isInitialized) return;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Draw();
            }

            DrawCircle(attackRange, Color.yellow);
        }

        private void DrawCircle(float radius, Color color, int segments = 32)
        {
            float angleStep = 360f / segments;

            Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;

                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );

                Debug.DrawLine(prevPoint, newPoint, color);

                prevPoint = newPoint;
            }
        }
    }
}