using System.Collections.Generic;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class BattleTickable : ITickable
    {
        [SerializeField, HideLabel] private BattleSetting setting;
        // biến kiểm tra đã khởi tạo chưa
        [HideInEditorMode, DisableInPlayMode]
        [SerializeField] private bool isInitialized = false;
        
        private Dice[] dices;
        private Cone[] cones;
        private int number;
        private HashSet<int> values = new HashSet<int>();

        public void Initialize(IQuery query)
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
                cones[i] = new Cone(i, setting, query);
            }

            isInitialized = true;
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

            DrawCircle(new Vector3(setting.center.x, setting.center.y), setting.weaponAttackRange, Color.yellow);
        }
        
        void DrawCircle(Vector3 center, float radius, Color color, int segments = 32)
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