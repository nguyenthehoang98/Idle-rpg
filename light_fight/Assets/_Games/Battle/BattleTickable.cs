using System;
using System.Collections;
using System.Collections.Generic;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    [Serializable]
    public class BattleTickable : ITickable
    {
        [HideInEditorMode, DisableInPlayMode] [SerializeField]
        private bool isInitialized = false;

        public event Action OnInitialized;
        private Dice[] dices;
        private Cone[] cones;
        private int number;
        private HashSet<int> values = new HashSet<int>();
        private Vector3 center;
        private float attackRange;

        public void Initialize(BattleShare share, BattleSetting setting, IQuery query)
        {
            this.attackRange = setting.weaponAttackRange;
            this.center = new Vector3(setting.center.x, setting.center.y);
            this.dices = new Dice[setting.totalDice];
            this.cones = new Cone[BattleConst.MAX_DICE_NUMBER];

            for (int i = 0; i < setting.totalDice; i++)
            {
                dices[i] = new Dice(share, setting.diceCooldown, setting.diceDelayTrigger);
                dices[i].OnTriggerDice += TriggerDice;
            }

            for (int i = 0; i < cones.Length; i++)
            {
                cones[i] = new Cone(share, i, setting, query);
            }

            CoroutineUtils.Run(share.owner, InitCoroutine());
        }

        private IEnumerator InitCoroutine()
        {
            yield return new WaitForSeconds(1);

            for (int i = 0; i < cones.Length; i++)
            {
                cones[i].Init();
                yield return new WaitForSeconds(0.075f);
            }

            yield return new WaitForSeconds(0.5f);

            float play = 0;
            for (int i = 0; i < cones.Length; i++)
            {
                play = cones[i].Play();
            }

            yield return new WaitForSeconds(play);

            yield return new WaitForSeconds(0.1f);

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
            if (!isInitialized) return new int[0];

            int[] numbers = new int[dices.Length];
            for (int i = 0; i < dices.Length; i++)
            {
                numbers[i] = dices[i].Value;
            }

            return numbers;
        }

        public void Tick(float dt)
        {
            if (!isInitialized) return;

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
            if (!isInitialized) return;

            for (int i = 0; i < cones.Length; i++)
            {
                cones[i].Draw();
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