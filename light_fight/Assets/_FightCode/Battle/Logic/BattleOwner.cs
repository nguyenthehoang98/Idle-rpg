using System.Collections.Generic;
using _FightCode.Battle.Model;
using _FightCode.Battle.Popup;
using _FightCode.Battle.View;
using _FightCode.Config;
using _KITSystem.Config;
using _KITSystem.Entity;
using _KITSystem.Popup;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _FightCode.Battle.Logic
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
        private MovementTickable movement;
        
        private readonly Dictionary<int, int> entityToAgent = new Dictionary<int, int>();
        private readonly Dictionary<int, Monster> entityToMonster = new Dictionary<int, Monster>();

        private MonsterConfig monsterConfig;
        
        private DiceControlView diceControl;
        private bool waveSpawnComplete = false;
        private int totalEntityInScene = 0;
        private int killed;
        
        private void Start()
        {
            Debug.Log(@"Tạo behaviour tree tạo các kịch bản test");
            Debug.Log(@"Cần xử lý các công thức tính sát thương, power...");
            Debug.Log(@"Cần có method check config, load & validate tất cả mà ko cần play 1 level hoặc vào game");
            Debug.Log(@"Các object (monster) chưa giải phóng bộ nhớ");
            // todo: assign
            TryGetTickable(out skill);
            TryGetTickable(out agent);
            TryGetTickable(out spawner);
            TryGetTickable(out battle);
            TryGetTickable(out monster);
            TryGetTickable(out movement);
            
            // todo: create instance logic
            IQuery query = new SkillQuery(agent);
            share = new BattleShare
            {
                owner = this,
                timeScale = loop,
                coneParent = new GameObject("ConeParent").transform,
                attractorParent = new GameObject("AttractorParent", typeof(RectTransform)).transform,
                agentGrid = agent,
            };
           
            Transform attract = share.attractorParent;
            attract.SetParent(canvas.transform);
            attract.SetAsFirstSibling();
            attract.transform.localPosition = Vector3.zero;
            attract.transform.localScale = Vector3.one;

            monsterConfig = ConfigManager.Get<MonsterConfig>();
            
            movement.Initialize();
            agent.Initialize();
            spawner.Initialize(1, OnCreateMonster);
            
            List<DiceView> diceViews = new List<DiceView>();
            for (int i = 0; i < setting.totalSlot; i++)
            {
                diceViews.Add(Object.Instantiate(setting.dice));
            }
            
            battle.OnInitialized += OnBattleInitialize;
            battle.Initialize(share, setting, query, diceViews);

            diceControl = DiceControlView.Instantiate(setting.diceControl, canvas.transform, diceViews);
            diceControl.OnInitialized += OnDiceInitialize;
            diceControl.OnSpeedChanged += battle.SetDiceSpeed;  
            
            // todo: reset global data + register event
            //SkillFactory.Initialize(skill, query);
            EntityManager.OnBehaviour += OnEntityBehaviour;
            spawner.OnWaveCompleted += OnWaveComplete;

            MonsterAnimation.Order = 1;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(share != null) share.timeScale = loop;
#endif
        }

        private void OnWaveComplete()
        {
            waveSpawnComplete = true;
        }

        private void OnEntityBehaviour(EntityChangedEvent parameter)
        {
            int entity = parameter.Entity;
            Monster m;
            
            /*switch (parameter.type)
            {
                case EntityManagerBehaviourType.BeHit:
                    
                    if (entityToMonster.TryGetValue(entity, out m))
                    {
                        Vector3 direction = parameter.Values[0].Vector3Value;
                        m.BeHit(direction);
                    }
                    
                    break;
                case EntityManagerBehaviourType.Removed:

                    killed++;
                    
                    if (entityToAgent.Remove(entity, out int agentId))
                    {
                        agent.DestroyAgent(agentId);
                    }

                    if (entityToMonster.Remove(entity, out m))
                    {
                        monster.RemoveMonster(m);
                    }

                    totalEntityInScene = entityToAgent.Count;
                    
                    SpawnAction();
                    
                    break;
            }*/
        }

        private void OnDestroy()
        {
            if (battle != null)
            {
                battle.OnInitialized -= OnBattleInitialize;
            }

            if (diceControl != null)
            {
                diceControl.OnInitialized -= OnDiceInitialize;
                if (battle != null) diceControl.OnSpeedChanged -= battle.SetDiceSpeed;
            }
            
            if (spawner != null)
            {
                spawner.OnWaveCompleted -= OnWaveComplete;
                spawner.Dispose();
            }

            EntityManager.OnBehaviour -= OnEntityBehaviour;
            EntityManager.Dispose();
            //SkillFactory.Dispose();
            movement?.Dispose();
            agent?.Dispose();
        }

        private void OnDiceInitialize()
        {
            StartGame();
        }

        private void OnBattleInitialize()
        {
            diceControl.Initialize(share, setting);
        }

        private void OnCreateMonster(RequestCreateMonster request)
        {
            /*Config.MonsterData monsterData = request.MonsterData;
            float2 position = request.Position;
            float radius = monsterData.radius;
            float speed = monsterData.moveSpeed;
            float stopDistance = monsterData.stopMoveDistance;
            int monsterId = monsterData.ID;
            int entity = EntityManager.CreateEntity();
                
            AgentData agentData = agent.CreateAgent(entity, position, radius, speed, setting.defaultAgentStopDistance + stopDistance);
            entityToAgent[entity] = agentData.agent;
                
            ComponentManager<HealthData>.Add(entity, new HealthData(20));
            ComponentManager<MonsterData>.Add(entity, new MonsterData(monsterId));

            Monster m = new Monster(share, monsterConfig, entity, monsterId, agentData.agent);
            entityToMonster[entity] = m;
            monster.AddMonster(m);*/
        }

        private async void StartGame()
        {
            await spawner.WaveSpawn();
            
            IsPaused = false;
        }

        private void OnDrawGizmos()
        {
            if (monster != null) monster.Draw();
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
                this.WaitInvoke(2f / share.timeScale, async () =>
                {
                    bool spawn = await spawner.WaveSpawn();
                    
                    if (!spawn)
                    {
                        IsPaused = true;
                        PopupManager.Instance.Push<BattleWinPopup>();
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
