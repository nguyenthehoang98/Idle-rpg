using System.Collections.Generic;
using System.Linq;
using _Game.Battle.Level;
using UnityEditor;
using UnityEngine;

namespace _Game.Battle.Checker.Editor
{
    public partial class LevelSpawnEditorWindow : BaseEditorWindowChart
    {
        private LevelInput input = new LevelInput { level = 1, fromLevel = 1, toLevel = 10 };
        private GenerateLevelData generateData = new GenerateLevelData();
        private bool drawWave = true;
        private bool drawBatch;
        private bool drawInterval;

        [MenuItem("Tools/Chart/Level Spawn")]
        public static void Open()
        {
            GetWindow<LevelSpawnEditorWindow>("Level Spawn");
        }
        
        [MenuItem("Tools/Validate/Level Spawn")]
        static void Validate()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(LevelSpawnSO).Name}");
            LevelSpawnSO[] array = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<LevelSpawnSO>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
            foreach (var item in array)
            {
                item.Validate();
            }
        }
        
        void DrawInput(bool isRange)
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            drawWave = EditorGUILayout.Toggle("Wave?", drawWave);
            drawBatch = EditorGUILayout.Toggle("Batch?", drawBatch);
            drawInterval = EditorGUILayout.Toggle("Interval?", drawInterval);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            if (isRange)
            {
                input.fromLevel = EditorGUILayout.IntField("From Level", input.fromLevel);
                input.toLevel = EditorGUILayout.IntField("To Level", input.toLevel);
            }
            else
            {
                input.level = EditorGUILayout.IntField("Level", input.level);
            }
        }

        private void DrawSingleTab()
        {
            DrawInput(isRange: false);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Preview Level", GUILayout.Height(32)))
            {
                string path = "Assets/_Sources/Battles/Levels/LevelSpawner_" + input.level + ".asset";
                LevelSpawnSO so = AssetDatabase.LoadAssetAtPath<LevelSpawnSO>(path);
                if (so != null)
                {
                    EnableChart();
                    chartData.Push(so, drawWave, drawBatch, drawInterval);
                }
                else
                {
                    LogErrorChart($"Không có level phù hợp với đường dẫn '{path}'");
                }
            }
        }

        private void DrawRangeTab()
        {
            DrawInput(isRange: true);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Preview Level", GUILayout.Height(32)))
            {
                List<LevelSpawnSO> list = new List<LevelSpawnSO>();
                for (int level = input.fromLevel; level <= input.toLevel; level++)
                {
                    string path = "Assets/SpawnConfig_" + level + ".asset";
                    LevelSpawnSO so = AssetDatabase.LoadAssetAtPath<LevelSpawnSO>(path);
                    list.Add(so);
                }

                if (list.Count > 0)
                {
                    EnableChart();
                    chartData.Push(list);
                }
                else
                {
                    LogErrorChart($"Không có level nào phù hợp [{input.fromLevel} : [{input.toLevel}]]");
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
                Build(generateData);
            }

            EditorGUILayout.Space(8);
        }

        protected override string[] ToolbarNames()
        {
            return new[] { "Single", "Range", "Generate" };
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
            else if (tabIndex == 2)
            {
                DrawGenerateTab();
            }
        }

        static void Build(GenerateLevelData generateData)
        {
            var samples = SampleCurve(generateData.curve, 60);
            var wavePowers = ComputeWavePower(samples, generateData.totalWave, generateData.totalPower);
            var config = CreateInstance<LevelSpawnSO>();
            for (int i = 0; i < generateData.totalWave; i++)
            {
                LevelSpawnSO.WaveSpawn wave = new LevelSpawnSO.WaveSpawn();
                wave.batches = new List<LevelSpawnSO.BatchSpawn>();
                wave.power = (int)wavePowers[i];
                wave.batches = new List<LevelSpawnSO.BatchSpawn>();
                config.waves.Add(wave);
            }

            SaveConfigAsset(config, generateData.level);
        }

        static void SaveConfigAsset(LevelSpawnSO so, int levelId)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save LevelSpawnConfig",
                $"SpawnConfig_{levelId}",
                "asset",
                "Choose location"
            );

            if (string.IsNullOrEmpty(path))
                return;

            AssetDatabase.CreateAsset(so, path);
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
    }

    public partial class LevelSpawnEditorWindow
    {
        private class LevelInput
        {
            public int level;
            public int fromLevel;
            public int toLevel;
        }
        
        private class GenerateLevelData
        {
            public int level;
            public int totalWave;
            public int totalPower;
            public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        }
    }

    public partial class ChartData
    {
        public void Push(LevelSpawnSO so, bool drawWave, bool drawBatch, bool drawInterval)
        {
            List<float> wavePowers = new List<float>();
            List<float> batchPowers = new List<float>();
            List<float> intervalPowers = new List<float>();
            foreach (var wave in so.waves)
            {
                wavePowers.Add(wave.power);
                foreach (var batch in wave.batches)
                {
                    batchPowers.Add(batch.power);
                    intervalPowers.Add(batch.power / batch.duration);
                }
            }

            points = new List<Point>();
            if (drawWave) points.Add(GetPoints(wavePowers, Vector2.zero, true, Color.green));
            if (drawBatch) points.Add(GetPoints(batchPowers, Vector2.zero, true, Color.yellow));
            if (drawInterval) points.Add(GetPoints(intervalPowers, Vector2.zero, true, Color.red));
        }

        public void Push(List<LevelSpawnSO> configs)
        {
            List<float> powers = new List<float>();
            foreach (var config in configs)
            {
                int power = 0;
                if (config != null)
                {
                    foreach (var wave in config.waves) power += wave.power;
                }

                powers.Add(power);
            }

            points = new List<Point>
            {
                GetPoints(powers, Vector2.zero, false, Color.yellow)
            };
        }
    }
}