using System;
using System.Collections.Generic;
using _Game.Battle.Utils;
using _Game.Scripts.Configs;
using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Editor
{
    public partial class MonsterEditorWindow : BaseEditorWindowChart
    {
        private MonsterConfig monsterConfig;
        private SkillConfig skillConfig;
        private bool drawPower = true;
        private bool drawHealth;
        private bool drawDefense;
        private bool drawAttack;
        private MonsterInput input = new MonsterInput
        {
            id = 1001,
            fromId = 1001,
            toId = 1001,
        };
        
        [MenuItem("Tools/Chart/Monster")]
        public static void Open()
        {
            GetWindow<MonsterEditorWindow>("Monster");
        }

        private void OnEnable()
        {
            monsterConfig = MonsterConfig.Instance;
            skillConfig = SkillConfig.Instance;
        }

        private void OnFocus()
        {
            monsterConfig = MonsterConfig.Instance;
            skillConfig = SkillConfig.Instance;
        }

        void DrawInput(bool isRange)
        {
            EditorGUILayout.LabelField("Monster Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            drawPower = EditorGUILayout.Toggle("Power?", drawPower);
            drawAttack = EditorGUILayout.Toggle("Attack?", drawAttack);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            drawHealth = EditorGUILayout.Toggle("Health?", drawHealth);
            drawDefense = EditorGUILayout.Toggle("Defense?", drawDefense);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            if (isRange)
            {
                input.fromId = EditorGUILayout.IntField("From Id", input.fromId);
                input.toId = EditorGUILayout.IntField("To Id", input.toId);
            }
            else
            {
                input.id = EditorGUILayout.IntField("Id", input.id);
            }
        }

        protected override string[] ToolbarNames()
        {
            return new[] { "Single", "Multiple" };
        }

        protected override void OnDrawTab(int tabIndex)
        {
            if (tabIndex == 0)
            {
                DrawSingleTab();
            }
            else if (tabIndex == 1)
            {
                DrawRangeTab();
            }
        }

        private void DrawSingleTab()
        {
            DrawInput(isRange: false);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Preview Level", GUILayout.Height(32)))
            {
                if (monsterConfig.Find(input.id, out var monsterData) &&
                    skillConfig.Find(monsterData.SkillId, out var skillData))
                {
                    EnableChart();
                    chartData.Push(monsterData, skillData,
                        drawPower, drawAttack, drawHealth, drawDefense
                    );
                }
                else
                {
                    LogErrorChart($"Không tìm thấy monster với id'{input.id}'");
                }
            }
        }

        private void DrawRangeTab()
        {
            DrawInput(isRange: true);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Preview Level", GUILayout.Height(32)))
            {
            }
        }
    }

    public partial class MonsterEditorWindow
    {
        private class MonsterInput
        {
            public int id;
            public int fromId;
            public int toId;
        }
    }

    public partial class ChartData
    {
        public void Push(MonsterConfig.MonsterData monsterData, SkillConfig.SkillData skillData,
            bool drawPower, bool drawAttack, bool drawHealth, bool drawDefense)
        {
            List<float> attacks = new List<float>();
            List<float> healths = new List<float>();
            List<float> defenses = new List<float>();
            List<float> powers = new List<float>();
            for (int i = 1; i <= 10; i++)
            {
                attacks.Add(monsterData.Attack(i));
                healths.Add(monsterData.Health(i));
                defenses.Add(monsterData.Defense(i));
                powers.Add(FormulaUtils.PowerMonster(monsterData, i, skillData));
            }

            points = new List<Point>();
            if (drawPower) points.Add(GetPoints(powers, Vector2.zero, true, Color.yellow));
            if (drawAttack) points.Add(GetPoints(attacks, Vector2.zero, true, Color.red));
            if (drawDefense) points.Add(GetPoints(defenses, Vector2.zero, true, Color.blue));
            if (drawHealth) points.Add(GetPoints(healths, Vector2.zero, true, Color.green));
        }
    }
}