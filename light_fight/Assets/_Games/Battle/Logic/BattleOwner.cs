using System.Collections.Generic;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.ExcelConfig;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Entity;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle.Logic
{
    public class BattleOwner : TickSystemOwner
    {
        [TitleGroup("Battle")]
        [SerializeField] private BattleSetting setting;
        [SerializeField] private Canvas canvas;

        private BattleShare share;
        private SkillTickable skill;
        private AgentTickable agent;
        private SpawnerTickable spawner;
        private BattleTickable battle;
        private MonsterTickable monster;
        
        private readonly Dictionary<int, int> entityToAgent = new Dictionary<int, int>();
        private readonly Dictionary<int, Monster> entityToMonster = new Dictionary<int, Monster>();

        private IDiceControlView diceControl;
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
            TryGetTickable(out monster);
            TryGetTickable(out MovementTickable movement);
            
            // todo: create instance logic
            IQuery query = new SkillQuery(agent);
            share = new BattleShare
            {
                owner = this,
                timeScale = loop,
                coneParent = new GameObject("ConeParent").transform,
                attractorParent = new GameObject("AttractorParent", typeof(RectTransform)).transform,
                agentGrid = agent,
                dices = new List<IDiceView>(),
                slots = new List<ISlotView>(),
            };
           
            Transform attract = share.attractorParent;
            attract.SetParent(canvas.transform);
            attract.SetAsFirstSibling();
            attract.transform.localPosition = Vector3.zero;
            attract.transform.localScale = Vector3.one;
            
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
                
                ComponentManager<HealthData>.Add(entity, new HealthData(20));
                ComponentManager<MonsterData>.Add(entity, new MonsterData(monsterId));

                Monster m = new Monster(share, setting, entity, monsterId, agentData.agent);
                entityToMonster[entity] = m;
                monster.AddMonster(m);
            });
            battle.OnInitialized += OnBattleInitialize;
            battle.Initialize(share, setting, query);

            diceControl = setting.diceControl.Instantiate(canvas.transform);
            diceControl.OnInitialized += OnDiceInitialize;
            diceControl.OnSpeedChanged += battle.SetDiceSpeed;  
            
            // todo: reset global data + register event
            SkillFactory.Initialize(skill, query);
            EntityManager.OnBehaviour += OnEntityBehaviour;
            spawner.OnWaveCompleted += OnWaveComplete;
        }

        private void OnWaveComplete()
        {
            waveSpawnComplete = true;
            SpawnAction();
        }

        private void OnEntityBehaviour(EntityManagerBehaviourParameters parameter)
        {
            int entity = parameter.entity;
            Monster m;
            
            switch (parameter.type)
            {
                case EntityManagerBehaviourType.BeHit:
                    if (entityToMonster.TryGetValue(entity, out m))
                    {
                        m.BeHit();
                    }
                    
                    break;
                case EntityManagerBehaviourType.Removed:
                    
                    killed++;
                    totalEntityInScene = entityToAgent.Count;
                    SpawnAction();

                    if (entityToAgent.Remove(entity, out int agentId))
                    {
                        agent.DestroyAgent(agentId);
                    }

                    if (entityToMonster.Remove(entity, out m))
                    {
                        monster.RemoveMonster(m);
                    }
                    break;
            }
        }

        private void OnDiceInitialize()
        {
            StartGame();
        }

        private void OnBattleInitialize()
        {
            diceControl.Initialize(share, setting);
        }

        private void StartGame()
        {
            spawner.WaveSpawn();
            IsPaused = false;
        }

        private void OnDrawGizmos()
        {
            if(!setting.enableVisualize)
            {
                if (battle != null) battle.Draw();
                if (monster != null) monster.Draw();
            }
        }

        private void OnGUI()
        {
            if (battle != null)
            {
                DrawCell("Killed " + killed, new Vector2(0.1f, 0.2f), 0.1f, Color.gray, Color.white);
            }
        }

        private void SpawnAction()
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