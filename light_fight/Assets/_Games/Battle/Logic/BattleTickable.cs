using System;
using System.Collections;
using _Games.Battle.Model;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
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
            this.diceNumbers = new int[setting.totalSlot];
            this.dices = new Dice[setting.totalSlot];
            this.slots = new Slot[BattleConst.MAX_DICE_NUMBER];
            this.attractors = new IAttractorView[setting.totalSlot];

            for (int i = 0; i < setting.totalSlot; i++)
            {
                int index = i;
                dices[i] = new Dice(share, setting);
                dices[i].OnTriggerDice += (i1) =>
                {
                    TriggerDice(index, i1);
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

        private void TriggerDice(int triggerDiceIndex, int triggerNumber)
        {
            if (!isInitialized)
            {
#if UNITY_EDITOR
                Debug.LogError("BattleTickable not initialized");
#endif
                return;
            }

            totalDiceActivate++;
            diceNumbers[triggerDiceIndex] = triggerNumber - 1;

            float delay = 0.5f;
            if (totalDiceActivate == dices.Length)
            {
                int[] stacks = new int[BattleConst.MAX_DICE_NUMBER];
                for (int i = 0; i < diceNumbers.Length; i++)
                {
                    int number = diceNumbers[i];
                    Slot slot = slots[number];
                    
                    stacks[number]++;
                    int stack = stacks[number]; // begin at 0;
                    
                    Vector3 start = share.dices[i].WorldPosition;
                    Vector3 end = share.slots[number].WorldPosition(stack);
                    Vector3 rot = share.slots[number].WorldEulerAngles(stack);
                    attractors[i].MoveTo(start, end, rot, stack * delay, setting.attractorFlyTime,
                        RandomUtils.Range(setting.minAttractorRadius, setting.maxAttractorRadius), 2.5f,
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
                        slot.Deactivate(setting.attractorFlyTime + delay);
                    }
                    else if (stack > 0 && !slot.IsPlaying)
                    {
                        slot.Activate(setting.attractorFlyTime + (delay + 0.1f) * stack);
                    }
                }

                Array.Clear(diceNumbers, 0, diceNumbers.Length);
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