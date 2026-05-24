using System;
using System.Collections;
using System.Collections.Generic;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle.Logic
{
    [Serializable]
    public class BattleTickable : ITickable
    {
        public event Action OnInitialized;

        private BattleSetting setting;
        private BattleShare share;
        private bool isInitialized = false;
        private Dice[] dices;
        private Slot[] slots;
        private IAttractorView[] attractors;
        private int totalDiceActivate;
        // {number:stack}
        private int[] diceNumberStacks = new int[BattleConst.MAX_DICE_NUMBER];
        // {index:number}
        private int[] diceNumbers;
        private Vector3 center;
        private float attackRange;

        public void Initialize(BattleShare share, BattleSetting setting, IQuery query)
        {
            this.share = share;
            this.setting = setting;
            this.attackRange = setting.weaponAttackRange;
            this.center = new Vector3(setting.center.x, setting.center.y);
            this.diceNumbers = new int[setting.totalDice];
            this.dices = new Dice[setting.totalDice];
            this.slots = new Slot[BattleConst.MAX_DICE_NUMBER];
            this.attractors = new IAttractorView[setting.totalDice];

            for (int i = 0; i < setting.totalDice; i++)
            {
                int index = i;
                dices[i] = new Dice(share, setting);
                dices[i].OnTriggerDice += (i1, f) =>
                {
                    TriggerDice(index, i1, f);
                };
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new Slot(share, i, setting, query);
            }

            for (int i = 0; i < attractors.Length; i++)
            {
                attractors[i] = setting.attractor.Instantiate(share.attractorParent);
            }

            CoroutineUtils.Run(share.owner, InitCoroutine(share.timeScale));
        }
        
        private IEnumerator InitCoroutine(float timeScale)
        {
            yield return new WaitForSeconds(1f / timeScale);

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Initialize();
                
                yield return new WaitForSeconds(0.1f / timeScale);
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

        private void TriggerDice(int triggerDiceIndex, int triggerNumber, float duration)
        {
            if (!isInitialized)
            {
#if UNITY_EDITOR
                Debug.LogError("BattleTickable not initialized");
#endif
                return;
            }

            totalDiceActivate++;
            diceNumberStacks[triggerNumber - 1]++;
            diceNumbers[triggerDiceIndex] = triggerNumber - 1;
            
            if (totalDiceActivate == dices.Length)
            {
                for (int i = 0; i < diceNumberStacks.Length; i++)
                {
                    Slot slot = slots[i];
                    if (diceNumberStacks[i] == 0 && slot.IsPlaying)
                    {
                        slot.Deactivate();
                    }
                }
                
                for (int dice = 0; dice < diceNumbers.Length; dice++)
                {
                    int number = diceNumbers[dice];
                    int stack = diceNumberStacks[number];
                    Slot slot = slots[number];
                    
                    if (stack != 0)
                    {
                        Vector3 start = share.dices[dice].WorldPosition;
                        Vector3 end = share.slots[number].WorldPosition;
                        Vector3 rot = share.slots[number].WorldEulerAngles;
                        attractors[dice].MoveTo(start, end, rot, 0, duration,
                            RandomUtils.Range(setting.minAttractorRadius, setting.maxAttractorRadius), 2.5f,
                            RandomUtils.Range(0.3f, 0.5f), () =>
                            {
                                if (!slot.IsPlaying) slot.Activate();
                                slot.DoStack(stack);
                            });
                    }
                }

                Array.Clear(diceNumbers, 0, diceNumbers.Length);
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