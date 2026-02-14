using System;
using System.Collections.Generic;
using _Game.Configs;
using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Editor
{
    public partial class MonsterEditorWindow : BaseEditorWindowChart
    {
        private MonsterConfig monsterConfig;
        
        private MonsterInput input = new MonsterInput
        {
            id = 1000101,
            fromId = 1000101,
            toId = 1000101,
        };
        
        [MenuItem("Tools/Chart/Monster")]
        public static void Open()
        {
            GetWindow<MonsterEditorWindow>("Monster");
        }

        private void OnEnable()
        {
            monsterConfig = MonsterConfig.Instance;
        }

        private void OnFocus()
        {
            monsterConfig = MonsterConfig.Instance;
        }

        void DrawInput(bool isRange)
        {
            EditorGUILayout.LabelField("Monster Settings", EditorStyles.boldLabel);
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
                if (monsterConfig.Find(input.id, out var monsterData))
                {
                    EnableChart();
                    chartData.Push(monsterData);
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
        public void Push(MonsterConfig.MonsterData monsterData)
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
                powers.Add(FormulaUtils.PowerMonster(monsterData, i));
            }
            
            points = new List<Point>
            {
                GetPoints(attacks, Vector2.zero, false, Color.red),
                GetPoints(defenses, new Vector2(0, -0.01f), false, Color.blue),
                GetPoints(healths, new Vector2(0, 0.01f), false, Color.green),
                GetPoints(powers, new Vector2(0, 0.0f), false, Color.yellow),
            };
        }
    }
}