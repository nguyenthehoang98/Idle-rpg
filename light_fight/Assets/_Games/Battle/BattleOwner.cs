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
    public class BattleOwner : TickSystemOwner
    {
        [TitleGroup("Battle")]
        [SerializeField] private BattleSetting setting;

        private BattleShare share;
        private SkillTickable skill;
        private AgentTickable agent;
        private SpawnerTickable spawner;
        private BattleTickable battle;
        private Dictionary<int, int> entityToAgent = new Dictionary<int, int>();

        private bool waveSpawnComplete = false;
        private int totalEntityInScene = 0;
        private int killed;
        
        private async void Start()
        {
            // todo: load instance data
            await KitConfigManager.Load(new string[]
            {
                "SkillConfig",
                "LevelConfig",
                "MonsterConfig",
            });
            
            // todo: assign
            TryGetTickable(out skill);
            TryGetTickable(out agent);
            TryGetTickable(out spawner);
            TryGetTickable(out battle);
            TryGetTickable(out MovementTickable movement);
            
            // todo: create instance logic
            IQuery query = new SkillQuery(agent);
            share = new BattleShare();
            share.owner = this;
            share.timeScale = loop;
            
            Transform coneParent = null;
            if (setting.mode == BattleMode.Default)
            {
                coneParent = new GameObject("ConeParent").transform;
                coneParent.parent = transform;
            }
            share.coneParent = coneParent;
            
            movement.Initialize();
            agent.Initialize();
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
            battle.OnInitialized += OnInitialize;
            battle.Initialize(share, setting, query);
            
            // todo: reset global data + register event
            
            SkillFactory.Initialize(skill, query);
            EntityManager.OnEntityRemoved += i =>
            {
                killed++;
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

        private void OnInitialize()
        {
            spawner.WaveSpawn();
            
            IsPaused = false;
        }

        private void OnDrawGizmos()
        {
            if (battle != null) battle.Draw();
        }

        private void OnGUI()
        {
            if (battle != null)
            {
                int[] numbers = battle.DiceNumbers();
                for (int i = 0; i < numbers.Length; i++)
                {
                    DrawCell(numbers[i].ToString(), new Vector2(0.1f + 0.1f * i, 0.1f), 0.08f, Color.gray, Color.white);
                }  
                
                DrawCell("Killed: " + killed, new Vector2(0.1f, 0.2f), 0.1f, Color.gray, Color.white);
            }
        }

        void SpawnAction()
        {
            if (totalEntityInScene == 0 && waveSpawnComplete)
            {
                this.WaitInvoke(2, () =>
                {
                    bool spawn = spawner.WaveSpawn();
                    if (!spawn)
                    {
                        IsPaused = true;
                        Debug.LogError("Complete");
                    }
                    else
                        waveSpawnComplete = false;
                });
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