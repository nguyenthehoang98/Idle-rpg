using System.Collections.Generic;
using _KITSystem.ExcelConfig;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Entity;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    public class TestOwner : MonoBehaviour
    {
        [SerializeField, Range(1, 25)] private int loop = 1;
        [SerializeField] private int targetFPS = 30;
        [SerializeField] private BattleSetting setting;

        [TitleGroup("Element")] 
        [SerializeField] private SkillTickable skill;
        [SerializeField] private AgentTickable agent;
        [SerializeField] private SpawnerTickable spawner;
        [SerializeField] private MovementTickable movement;

        private Dictionary<int, int> entityToAgent = new Dictionary<int, int>();
        private ITickable[] tickables;
        private BattleLogic logic;
        private float tickInterval;
        private float accumulator;

        private void Awake()
        {
            Application.runInBackground = true;
            tickInterval = 1f / targetFPS;

            tickables = new ITickable[5] { spawner, agent, movement, skill, null };
        }

        private async void Start()
        {
            await KitConfigManager.Load(new string[]
            {
                "SkillConfig",
                "LevelConfig",
                "MonsterConfig",
            });

            IQuery query = new SkillQuery(agent);
            
            SkillFactory.Initialize(skill, query);
            
            spawner.Initialize(1, request =>
            {
                float2 position = request.Position;
                float radius = request.Radius;
                
                int entity = EntityManager.CreateEntity();
                int monsterId = request.MonsterID;
                AgentData agentData = agent.CreateAgent(entity, position, radius);
                entityToAgent[entity] = agentData.agent;
                
                ComponentManager<HealthData>.Add(entity, new HealthData(10));
                ComponentManager<MonsterData>.Add(entity, new MonsterData(monsterId));
            });
            
            logic = new BattleLogic(setting, query);
            tickables[4] = logic;
            
            spawner.WaveSpawn();
            
            bool waveSpawnComplete = false;
            int totalEntityInScene = 0;
            void SpawnAction()
            {
                if (totalEntityInScene == 0 && waveSpawnComplete)
                {
                    this.WaitInvoke(2, () =>
                    {
                        bool spawn = spawner.WaveSpawn();
                        if (!spawn)
                            Debug.LogError("Complete");
                        else
                            waveSpawnComplete = false;
                    });
                }
            }

            EntityManager.OnEntityRemoved += i =>
            {
                totalEntityInScene = entityToAgent.Count;
                SpawnAction();
            };
            spawner.OnWaveCompleted += () =>
            {
                waveSpawnComplete = true;
                SpawnAction();
            };
            EntityManager.OnEntityRemoved += EntityRemoved;
        }

        private void OnDrawGizmos()
        {
            if (logic == null) return;

            logic.Draw();
        }

        private void OnGUI()
        {
            if (logic == null) return;

            int[] numbers = logic.DiceNumbers();
            for (int i = 0; i < numbers.Length; i++)
            {
                DrawCell(numbers[i].ToString(), new Vector2(0.1f + 0.1f * i, 0.2f), 0.08f, Color.gray, Color.white);
            }
        }

        private void Update()
        {
            if (logic == null) return;

            accumulator += Time.deltaTime * loop;
            float f = tickInterval;
            while (accumulator >= f)
            {
                for (int i = 0; i < tickables.Length; i++)
                {
                    tickables[i].Tick(f);
                }
                
                accumulator -= f;
            }
        }

        private void EntityRemoved(int entity)
        {
            if (entityToAgent.Remove(entity, out int agentId)) agent.DestroyAgent(agentId);
        }
        
        private void DrawCell(string text, Vector2 normalizedPos, float normalizedSize, Color background,
            Color textColor)
        {
            // scale theo màn hình
            float size = Mathf.Min(Screen.width, Screen.height) * normalizedSize;

            Rect rect = new Rect(
                normalizedPos.x * Screen.width - size * 0.5f,
                normalizedPos.y * Screen.height - size * 0.5f,
                size,
                size);

            // background
            Color oldColor = GUI.color;
            GUI.color = background;
            GUI.Box(rect, GUIContent.none);

            // text
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = Mathf.RoundToInt(size * 0.35f);
            style.normal.textColor = textColor;

            GUI.Label(rect, text, style);

            GUI.color = oldColor;
        }
    }
}