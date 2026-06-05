using System;
using System.Collections;
using System.Collections.Generic;
using _FightCode.Battle.Model;
using _FightCode.Battle.View;
using _FightCode.Utils;
using _KITSystem.Entity;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _FightCode.Battle.Logic
{
    [Serializable]
    public class BattleTickable : ITickable
    {
        public event Action OnInitialized;

        private bool isInitialized;
        private Dice[] dices;
        private Slot[] slots;
        private DiceView[] diceViews;
        private SlotView[] slotViews;
        private AttractorView[] attractors;
        private float attractorFlyTime;
        private float minAttractorRadius;
        private float maxAttractorRadius;
        private int totalDiceActivate;
        // {index:number}
        private int[] diceNumbers;

        public void Initialize(BattleShare share, BattleSetting setting, IQuery query, List<DiceView> diceViews, List<SlotView> slotViews)
        {
            this.minAttractorRadius = setting.minAttractorRadius;
            this.maxAttractorRadius = setting.maxAttractorRadius;
            this.attractorFlyTime = setting.attractorFlyTime;
            this.diceNumbers = new int[setting.totalSlot];
            this.dices = new Dice[setting.totalSlot];
            this.slots = new Slot[Const.MAX_DICE_NUMBER];
            this.attractors = new AttractorView[setting.totalSlot];
            this.diceViews = diceViews.ToArray();
            this.slotViews = slotViews.ToArray();

            for (int i = 0; i < setting.totalSlot; i++)
            {
                int index = i;
                dices[i] = new Dice(setting, diceViews[index]);
                dices[i].OnTriggerDice += (i1) =>
                {
                    TriggerDice(index, i1);
                };
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new Slot(i, slotViews[i], share, setting, query);
            }

            for (int i = 0; i < attractors.Length; i++)
            {
                attractors[i] = Object.Instantiate(setting.attractor, share.attractorParent);
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

        private void TriggerDice(int triggerDiceIndex, int triggerNumber)
        {
            if (!isInitialized) return;

            totalDiceActivate++;
            diceNumbers[triggerDiceIndex] = triggerNumber - 1;

            float delay = 0.2f;
            if (totalDiceActivate == dices.Length)
            {
                int[] stacks = new int[Const.MAX_DICE_NUMBER];
                for (int i = 0; i < diceNumbers.Length; i++)
                {
                    int number = diceNumbers[i];
                    Slot slot = slots[number];
                    
                    stacks[number]++;
                    int stack = stacks[number]; // begin at 0;
                    
                    Vector3 start = diceViews[i].WorldPosition + Vector3.up * 0.1f;
                    Vector3 end = slotViews[number].WorldPosition(stack);
                    Vector3 rot = slotViews[number].WorldEulerAngles(stack);
                    attractors[i].MoveTo(start, end, rot, stack * delay, attractorFlyTime,
                        RandomUtils.Range(minAttractorRadius, maxAttractorRadius), 2.5f,
                        RandomUtils.Range(0.3f, 0.5f), () =>
                        {
                            slot.DoStack(stack);
                        });
                }
                
                for (int i = 0; i < stacks.Length; i++)
                {
                    int stack = stacks[i];
                    Slot slot = slots[i];
                    
                    if (stack == 0 && slot.IsPlaying)
                    {
                        slot.Deactivate(attractorFlyTime + delay);
                    }
                    else if (stack > 0 && !slot.IsPlaying)
                    {
                        slot.Activate(attractorFlyTime + (delay + 0.1f) * stack);
                    }
                }

                Array.Clear(diceNumbers, 0, diceNumbers.Length);
                totalDiceActivate = 0;
            }
        }

        public void SetDiceSpeed(float speed)
        {
            for (int i = 0; i < dices.Length; i++)
                dices[i].SetSpeed(speed);
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
    }
}