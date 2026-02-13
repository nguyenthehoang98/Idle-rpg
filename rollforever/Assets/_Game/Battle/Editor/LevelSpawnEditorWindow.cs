using System;
using System.Collections.Generic;
using System.Linq;
using Syrus.Plugins.ChartEditor;
using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Editor
{
    public class LevelSpawnEditorWindow : EditorWindow
    {
        #region Const

        private const int DEFAULT_LEVEL = 1;
        private const int Samples = 60;

        #endregion

        #region Types

        private enum Tab
        {
            Single,
            Range,
            Preview
        }

        private class LevelInput
        {
            public int level;
            public int fromLevel;
            public int toLevel;

            public static LevelInput Single()
            {
                return new LevelInput
                {
                    level = DEFAULT_LEVEL,
                };
            }

            public static LevelInput Range()
            {
                return new LevelInput
                {
                    fromLevel = DEFAULT_LEVEL,
                    toLevel = DEFAULT_LEVEL + 9,
                };
            }
        }

        private class GenerateLevelData
        {
            public int level;
            public int totalWave;
            public int totalPower;
            public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        }

        #endregion

        #region Chart

        private bool shouldDrawChart = false;
        private bool showHelpbox = false;
        private string errorMessage = "";
        private ChartData chartData = new ChartData();
        
        public class ChartData
        {
            public float heightNormalize = 100;
            public Rect layoutRect;
            public Color backgroundColor = new Color(0.5f,0.5f,0.5f, 0.5f);
            public List<Point> points = new List<Point>();

            public void Push(LevelSpawnConfig config)
            {
                List<float> wavePowers = new List<float>();
                List<float> batchPowers = new List<float>();
                List<float> intervalPowers = new List<float>();
                foreach (var wave in config.waves)
                {
                    wavePowers.Add(wave.power);
                    foreach (var batch in wave.batches)
                    {
                        batchPowers.Add(batch.power);
                        intervalPowers.Add(batch.power / batch.duration);
                    }
                }

                Point GetPoints(List<float> list, Vector2 offset, Color color)
                {
                    Point p = new Point();
                    p.color = color;
                    float max = list.Max();
                    float total = list.Count - 1;
                    for (int i = 0; i < list.Count; i++)
                    {
                        p.points.Add(new Vector2(i / total, list[i] / max) + offset);
                    }

                    return p;
                }

                points = new List<Point>
                {
                    GetPoints(wavePowers, Vector2.zero, Color.green),
                    GetPoints(batchPowers, Vector2.zero, Color.yellow),
                    GetPoints(intervalPowers, new Vector2(0, 0.01f), Color.red),
                };
            }

            public void Push(List<LevelSpawnConfig> configs, Color color)
            {
                List<int> powers = new List<int>();
                foreach (var config in configs)
                {
                    int power = 0;
                    if (config != null)
                    {
                        foreach (var wave in config.waves) power += wave.power;
                    }

                    powers.Add(power);
                }
                
                Point p = new Point();
                p.color = color;
                float max = powers.Max();
                float total = powers.Count - 1;
                for (int i = 0; i < powers.Count; i++)
                {
                    p.points.Add(new Vector2(i / total, powers[i] / max));
                }

                points = new List<Point>
                {
                    p
                };
            }
            
            public class Point
            {
                public List<Vector2> points = new List<Vector2>();
                public Color color = Color.white;

                public Vector2[] NormalizePoints(float scale)
                {
                    Vector2[] vector2s = new Vector2[points.Count];
                    for (int i = 0; i < vector2s.Length; i++)
                    {
                        vector2s[i] = points[i] * new Vector2(1f, 1f) * scale;
                    }

                    return vector2s;
                }
            }
        }
        
        public class ChartView : IDisposable
        {
            private ChartData chartData;
            
            public ChartView(ChartData chartData)
            {
                this.chartData = chartData;
                Rect rect = this.chartData.layoutRect;
                rect.size = new Vector2(rect.size.x, rect.size.y);
                rect.position = new Vector2(rect.position.x, rect.position.y);
                GUIChartEditor.BeginChart(
                    chartData.layoutRect, chartData.backgroundColor,
                    GUIChartEditorOptions.ChartBounds(-5, 105, -5, 105),
                    GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft),
                    GUIChartEditorOptions.ShowAxes(Color.white),
                    GUIChartEditorOptions.ShowGrid(10f, 10f, Color.grey, true)
                );
            }

            public void DrawChart()
            {
                foreach (var p in chartData.points)
                {
                    var points = p.NormalizePoints(chartData.heightNormalize);
                    GUIChartEditor.PushLineChart(points, p.color);
                    foreach (var vector2 in points)
                    {
                        GUIChartEditor.PushPoint(vector2, Color.blue);
                    }
                }
            }
            
            public void Dispose()
            {
                GUIChartEditor.EndChart(); 
            }
        }

        #endregion
        
        #region State

        private Tab currentTab;

        private LevelInput singleInput = LevelInput.Single();
        private LevelInput rangeInput  = LevelInput.Range();
        private GenerateLevelData generateData = new GenerateLevelData();

        #endregion

        #region Menu

        [MenuItem("Tools/Chart/Level Spawn")]
        public static void Open()
        {
            GetWindow<LevelSpawnEditorWindow>("Level Spawn");
        }

        #endregion

        #region Unity

        private void OnGUI()
        {
            DrawToolbar();

            switch (currentTab)
            {
                case Tab.Single:
                    DrawSingleTab();
                    break;

                case Tab.Range:
                    DrawRangeTab();
                    break;

                case Tab.Preview:
                    DrawGenerateTab();
                    break;
            }

            if (shouldDrawChart)
            {
                Rect chartRect = GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true)
                );

                chartData.layoutRect = chartRect;

                using (var view = new ChartView(chartData))
                {
                    view.DrawChart();
                }
            }
            else if (showHelpbox)
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }

        #endregion

        #region Toolbar

        private void DrawToolbar()
        {
            int prev = (int)currentTab;
            currentTab = (Tab)GUILayout.Toolbar(
                (int)currentTab,
                new[] { "Single", "Range", "Generate" }
            );

            if (prev != (int)currentTab)
            {
                showHelpbox = false;
                shouldDrawChart = false;
            }

            EditorGUILayout.Space(10);
        }

        #endregion

        #region Tabs UI

        private void DrawSingleTab()
        {
            DrawLevelInput(singleInput, isRange: false);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Preview Level", GUILayout.Height(32)))
            {
                if (ValidateSingle(singleInput))
                {
                    string path = "Assets/SpawnConfig_" + singleInput.level + ".asset";
                    LevelSpawnConfig config = AssetDatabase.LoadAssetAtPath<LevelSpawnConfig>(path);
                    if (config != null)
                    {
                        shouldDrawChart = true;
                        showHelpbox = false;
                        chartData.Push(config);
                    }
                    else
                    {
                        showHelpbox = true;
                        errorMessage = $"Không có level phù hợp với đường dẫn '{path}'";
                    }
                }
            }
        }

        private void DrawRangeTab()
        {
            DrawLevelInput(rangeInput, isRange: true);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Preview Level", GUILayout.Height(32)))
            {
                if (ValidateRange(rangeInput))
                {
                    List<LevelSpawnConfig> list = new List<LevelSpawnConfig>();
                    for (int level = rangeInput.fromLevel; level <= rangeInput.toLevel; level++)
                    {
                        string path = "Assets/SpawnConfig_" + level + ".asset";
                        LevelSpawnConfig config = AssetDatabase.LoadAssetAtPath<LevelSpawnConfig>(path);
                        list.Add(config);
                    }

                    if (list.Count > 0)
                    {
                        shouldDrawChart = true;
                        showHelpbox = false;
                        chartData.Push(list, Color.yellow);                        
                    }
                    else
                    {
                        showHelpbox = true;
                        errorMessage = $"Không có level nào phù hợp [{rangeInput.fromLevel} : [{rangeInput.toLevel}]]";
                    }
                }
            }
        }

        private void DrawGenerateTab()
        {
            EditorGUILayout.LabelField("Level Info", EditorStyles.boldLabel);

            EditorGUILayout.Space(4);
            generateData.level = EditorGUILayout.IntField("Level", generateData.level);
            generateData.totalWave = EditorGUILayout.IntField("Total Wave", generateData.totalWave);
            generateData.totalPower = EditorGUILayout.IntField("Total Power", generateData.totalPower);
            generateData.curve = EditorGUILayout.CurveField("Curve", generateData.curve);

            if (GUILayout.Button("Generate", GUILayout.Height(28)))
            {
                Build();
            }

            EditorGUILayout.Space(8);
        }

        #endregion

        #region Shared UI

        private static void DrawLevelInput(LevelInput input, bool isRange)
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (isRange)
            {
                input.fromLevel = EditorGUILayout.IntField("From Level", input.fromLevel);
                input.toLevel   = EditorGUILayout.IntField("To Level", input.toLevel);
            }
            else
            {
                input.level = EditorGUILayout.IntField("Level", input.level);
            }
        }

        #endregion

        #region Validation

        private static bool ValidateSingle(LevelInput input)
        {
            if (input.level <= 0)
            {
                ShowError("Level must be greater than 0");
                return false;
            }

            return ValidateCommon(input);
        }

        private static bool ValidateRange(LevelInput input)
        {
            if (input.fromLevel <= 0 || input.toLevel <= 0)
            {
                ShowError("Level must be greater than 0");
                return false;
            }

            if (input.fromLevel > input.toLevel)
            {
                ShowError("From Level must be less than or equal To Level");
                return false;
            }

            return ValidateCommon(input);
        }

        private static bool ValidateCommon(LevelInput input)
        {
            return true;
        }

        private static void ShowError(string message)
        {
            EditorUtility.DisplayDialog("Invalid Input", message, "OK");
        }

        #endregion

        #region Preview Logic

        private void Build()
        {
            var samples = SampleCurve(generateData.curve, Samples);
            var wavePowers = ComputeWavePower(samples, generateData.totalWave, generateData.totalPower);
            var config = CreateInstance<LevelSpawnConfig>();
            for (int i = 0; i < generateData.totalWave; i++)
            {
                LevelSpawnConfig.WaveSpawn wave = new LevelSpawnConfig.WaveSpawn();
                wave.batches = new List<LevelSpawnConfig.BatchSpawn>();
                wave.power = (int)wavePowers[i];
                wave.batches = new List<LevelSpawnConfig.BatchSpawn>();
                config.waves.Add(wave);
            }

            SaveConfigAsset(config, generateData.level);
        }
        
        static void SaveConfigAsset(LevelSpawnConfig config, int levelId)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save LevelSpawnConfig",
                $"SpawnConfig_{levelId}",
                "asset",
                "Choose location"
            );

            if (string.IsNullOrEmpty(path))
                return;

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static float[] ComputeWavePower(float[] samples, int waveCount, float totalPower)
        {
            int samplesPerWave = samples.Length / waveCount;
            float[] waveEnergy = new float[waveCount];

            for (int w = 0; w < waveCount; w++)
            {
                int start = w * samplesPerWave;
                int end = w == waveCount - 1
                    ? samples.Length
                    : start + samplesPerWave;

                float sumSquares = 0f;
                int count = 0;

                for (int i = start; i < end; i++)
                {
                    sumSquares += samples[i] * samples[i];
                    count++;
                }

                waveEnergy[w] = Mathf.Sqrt(sumSquares / count); // RMS
            }

            float totalEnergy = 0f;
            foreach (var e in waveEnergy)
                totalEnergy += e;

            float[] wavePower = new float[waveCount];
            for (int i = 0; i < waveCount; i++)
                wavePower[i] = totalPower * (waveEnergy[i] / totalEnergy);

            return wavePower;
        }

        static float[] SampleCurve(AnimationCurve curve, int samples)
        {
            float[] values = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (samples - 1f);
                values[i] = Mathf.Max(0, curve.Evaluate(t));
            }
            return values;
        }

        #endregion
    }
}
