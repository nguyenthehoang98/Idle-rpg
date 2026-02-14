using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Editor
{
    [CustomEditor(typeof(LevelSpawnConfig))]
    public class LevelSpawnInspector : UnityEditor.Editor
    {
        private SerializedProperty wavesProp;
        private int selectedWave = 0;
        private int selectedBatch = -1;
        private Dictionary<int, bool> monsterFoldout = new Dictionary<int, bool>();
        private const float MIN_BTN_WIDTH = 135f;
        private const float BTN_HEIGHT = 26f;
        private const float SPACING = 6f;

        public static readonly Color[] DebugColors =
        {
            new Color(1f, 0f, 0f), // Red
            new Color(0f, 1f, 0f), // Green
            new Color(0f, 0f, 1f), // Blue
            new Color(1f, 1f, 0f), // Yellow
            new Color(1f, 0f, 1f), // Magenta
            new Color(0f, 1f, 1f), // Cyan

            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 1f), // Purple
            new Color(0f, 0.5f, 1f), // Sky Blue
            new Color(0.5f, 1f, 0f), // Lime
            new Color(1f, 0f, 0.5f), // Pink
            new Color(0f, 1f, 0.5f), // Mint

            new Color(0.6f, 0.3f, 0.1f), // Brown
            new Color(0.3f, 0.3f, 0.3f), // Dark Gray
            new Color(0.6f, 0.6f, 0.6f), // Light Gray
            new Color(0.2f, 0.6f, 0.4f), // Teal Green
            new Color(0.6f, 0.2f, 0.4f), // Rose
            new Color(0.4f, 0.4f, 0.8f), // Lavender
            new Color(0.8f, 0.4f, 0.4f), // Soft Red
            new Color(0.4f, 0.8f, 0.4f), // Soft Green
        };

        private void OnEnable()
        {
            wavesProp = serializedObject.FindProperty("waves");
        }

        private void OnDisable()
        {
            if (target is LevelSpawnConfig config) config.Disable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetDuration"));
            EditorGUILayout.Space(10);

            DrawWaveSelector();
            EditorGUILayout.Space(10);

            DrawWaveDetail();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawWaveSelector()
        {
            EditorGUILayout.LabelField("Waves", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("+ Add Wave", GUILayout.Height(22)))
            {
                wavesProp.arraySize++;
                wavesProp.GetArrayElementAtIndex(wavesProp.arraySize - 1)
                    .FindPropertyRelative("batches").arraySize = 0;

                selectedWave = wavesProp.arraySize - 1;
                selectedBatch = -1;
            }

            EditorGUILayout.Space(6);

            int total = wavesProp.arraySize;
            if (total == 0)
            {
                EditorGUILayout.HelpBox("No waves yet.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            DrawResponsiveButtonGrid(
                total,
                (index) => "W" + (index + 1) +
                           $" $[{wavesProp.GetArrayElementAtIndex(index).FindPropertyRelative("power").intValue}]",
                (index) => selectedWave == index,
                (index) =>
                {
                    selectedWave = index;
                    selectedBatch = -1;
                },
                Color.yellow, GetOtherColor
            );

            Color GetOtherColor(int index)
            {
                var waveProp = wavesProp.GetArrayElementAtIndex(index);
                var batchesProp = waveProp.FindPropertyRelative("batches");
                if (batchesProp.arraySize == 0)
                {
                    return Color.red;
                }
                    
                for (int i = 0; i < batchesProp.arraySize; i++)
                {
                    int enemies = batchesProp.GetArrayElementAtIndex(i).FindPropertyRelative("enemies").arraySize;
                    if (enemies > 0)
                    {
                        return EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    }
                }
                
                return Color.red;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawWaveDetail()
        {
            if (selectedWave < 0 || selectedWave >= wavesProp.arraySize)
                return;

            SerializedProperty wave = wavesProp.GetArrayElementAtIndex(selectedWave);

            // Remove wave
            if (GUILayout.Button("Remove This Wave"))
            {
                wavesProp.DeleteArrayElementAtIndex(selectedWave);
                selectedWave = Mathf.Clamp(selectedWave - 1, 0, wavesProp.arraySize - 1);
                selectedBatch = -1;
                return;
            }

            SerializedProperty powerProp = wave.FindPropertyRelative("power");
            int power = powerProp.intValue;

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Wave {selectedWave + 1} Details $Power [{power}]",
                EditorStyles.boldLabel);

            SerializedProperty batches = wave.FindPropertyRelative("batches");

            EditorGUILayout.Space(10);

            DrawBatchSelector(batches);

            DrawBatchDetail(power, batches);

            EditorGUILayout.EndVertical();
        }

        private void DrawBatchSelector(SerializedProperty batches)
        {
            EditorGUILayout.LabelField("Batches", EditorStyles.boldLabel);

            if (GUILayout.Button("+ Add Batch", GUILayout.Height(22)))
            {
                int last = batches.arraySize;
                batches.arraySize++;
                batches.GetArrayElementAtIndex(last).boxedValue = new LevelSpawnConfig.BatchSpawn();
                selectedBatch = batches.arraySize - 1;
            }

            EditorGUILayout.Space(6);

            int total = batches.arraySize;

            if (total == 0)
            {
                EditorGUILayout.HelpBox("No batches yet.", MessageType.Info);
                return;
            }

            DrawResponsiveButtonGrid(total,
                (index) =>
                {
                    var prop = batches.GetArrayElementAtIndex(index).FindPropertyRelative("power");
                    if (prop == null)
                    {
                        return "N/A";
                    }

                    return "B" + (index + 1) + $" $[{prop.intValue}]";
                },
                (index) => selectedBatch == index,
                (index) => selectedBatch = index,
                highlightColor: Color.cyan, i => EditorGUIUtility.isProSkin ? Color.white : Color.black
            );
        }

        private void DrawBatchDetail(int powerInput, SerializedProperty batches)
        {
            if (selectedBatch < 0 || selectedBatch >= batches.arraySize)
                return;
            
            SerializedProperty batch = batches.GetArrayElementAtIndex(selectedBatch);

            int totalPower = 0;
            for (int i = 0; i < batches.arraySize; i++)
            {
                if (i == selectedBatch) continue;
                totalPower += batches.GetArrayElementAtIndex(i).FindPropertyRelative("power").intValue;
            }
            
            int remainingPower = powerInput - totalPower;
            
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("Remove This Batch"))
            {
                batches.DeleteArrayElementAtIndex(selectedBatch);
                selectedBatch = Mathf.Clamp(selectedBatch - 1, 0, batches.arraySize - 1);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(batch.FindPropertyRelative("isBoosWave"));
            EditorGUILayout.PropertyField(batch.FindPropertyRelative("waitTimeSpawn"));
            EditorGUILayout.PropertyField(batch.FindPropertyRelative("duration"));
            EditorGUILayout.IntSlider(
                batch.FindPropertyRelative("power"), 0, remainingPower,
                new GUIContent("Power target")
            );

            EditorGUILayout.Space(10);

            DrawMonsterWeights(selectedBatch, batch);

            EditorGUILayout.EndVertical();
        }

        private void DrawMonsterWeights(int batchIndex, SerializedProperty batch)
        {
            SerializedProperty enemies = batch.FindPropertyRelative("enemies");

            if (!monsterFoldout.ContainsKey(batchIndex))
                monsterFoldout[batchIndex] = false;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            monsterFoldout[batchIndex] = EditorGUILayout.Foldout(
                monsterFoldout[batchIndex],
                $"Monster Weights ({enemies.arraySize})",
                true
            );

            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                int last = enemies.arraySize;
                enemies.arraySize++;
                enemies.GetArrayElementAtIndex(last).boxedValue = new LevelSpawnConfig.EnemySpawn
                {
                    weight = 1,
                    id = 1,
                    level = 1,
                };
            }

            EditorGUILayout.EndHorizontal();

            if (monsterFoldout[batchIndex])
            {
                EditorGUILayout.Space(6);

                for (int i = 0; i < enemies.arraySize; i++)
                {
                    SerializedProperty w = enemies.GetArrayElementAtIndex(i);
                    SerializedProperty powerProp = w.FindPropertyRelative("power");

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Monster {i + 1}", EditorStyles.boldLabel);

                    if (GUILayout.Button("X", GUILayout.Width(22)))
                    {
                        enemies.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(w.FindPropertyRelative("id"));
                    EditorGUILayout.PropertyField(w.FindPropertyRelative("level"));
                    EditorGUILayout.PropertyField(w.FindPropertyRelative("weight"));
                    w.FindPropertyRelative("weight").intValue = math.max(
                        0, w.FindPropertyRelative("weight").intValue
                    );

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawResponsiveButtonGrid(
            int total,
            Func<int, string> labelFunc,
            Func<int, bool> isSelectedFunc,
            Action<int> onClick,
            Color highlightColor, Func<int, Color> otherColor)
        {
            float viewWidth = EditorGUIUtility.currentViewWidth - 10;

            int columns = Mathf.Max(1, Mathf.FloorToInt((viewWidth + SPACING) / (MIN_BTN_WIDTH + SPACING)));

            float btnWidth = (viewWidth - SPACING * (columns - 1)) / columns;

            int rows = Mathf.CeilToInt(total / (float)columns);

            for (int r = 0; r < rows; r++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int c = 0; c < columns; c++)
                {
                    int index = r * columns + c;
                    if (index >= total)
                    {
                        GUILayout.Space(btnWidth + SPACING);
                        continue;
                    }

                    GUIStyle style = new GUIStyle(GUI.skin.button);

                    if (isSelectedFunc(index))
                    {
                        style.fontStyle = FontStyle.Bold;
                        style.normal.textColor = highlightColor;
                    }
                    else
                    {
                        style.normal.textColor = otherColor(index);
                    }

                    if (GUILayout.Button(labelFunc(index), style, GUILayout.Width(btnWidth),
                            GUILayout.Height(BTN_HEIGHT)))
                    {
                        onClick(index);
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);
            }
        }
    }
}