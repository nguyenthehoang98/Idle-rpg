/*using System.Collections.Generic;
using _KITSystem.EventBus;
using _KITSystem.ExcelConfig;
using _KITSystem.Grid;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Entity;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [TitleGroup("Settings")]
        [SerializeField, Range(0.1f, 1.0f)] private float duration = 0.6f;
        [SerializeField, Range(1.1f, 2.0f)] private float minRadius = 1.1f;
        [SerializeField, Range(1.1f, 2.0f)] private float maxRadius = 2.0f;
        [SerializeField] private float timeScale = 1;
        [SerializeField] private bool unlockAll;
        [SerializeField, Range(1, 4)] private int totalDice = 2;

        [TitleGroup("Prefabs")]
        [SerializeField] private ArcMove arcMovePrefab;
        [SerializeField] private BattleLevel battleLevelPrefab;
        [SerializeField] private UIBattleControlDiceSpeed controlDicePrefab;

        [TitleGroup("Elements")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private TickSystemOwner tickSystemOwner;
        
        [TitleGroup("Debug")]
        [SerializeField, DisableIf("@true")]
#if UNITY_EDITOR
        private SerializableDictionary<int, int> entityToAgent = new SerializableDictionary<int, int>();
#else
        private Dictionary<int, int> entityToAgent = new Dictionary<int, int>();        
#endif
        private List<ArcMove> listArcs = new List<ArcMove>();
        private readonly int[] numbers = new int[BattleConst.MAX_DICE_NUMBER];
        private readonly bool[] triggers = new bool[BattleConst.MAX_DICE_NUMBER];

        private UIBattleControlDiceSpeed controlDice;
        private BattleLevel battleLevel;
        private AgentTickable agentTickable;
        
        async void Awake()
        {
            Debug.Log(@"Thiết kế 1 phiên bản chỉ chạy logic ko bao gồm UI để có thể stresstest được");
            
            Debug.Log(@"Nâng cấp hơn thử tính Dmg xem agent có khả năng chết trong tương lai ko? nếu có thì sẽ ignore sang agent khác, cái này phải có 1 system riêng (lưu flag vào AgentData)");
            
            Debug.Log(@"UIBattleDiceSlot nên dùng queue để tính number dice. có thể config độ khó theo level theo các trường\n
- tỉ lệ quay vào ô chứa trang bị\n
	+ Tỉ lệ lặp lại ô chứa trang bị theo level, power. level max =50% chả hạn\n
- tỉ lệ quay vào ô không chứa trang bị");

            await KitConfigManager.Load(new string[]
            {
                "SkillConfig",
                "LevelConfig",
                "MonsterConfig",
            });
            
            tickSystemOwner.TryGetTickable(out SkillTickable skillTickable);
            tickSystemOwner.TryGetTickable(out agentTickable);
            tickSystemOwner.TryGetTickable(out SpawnerTickable spawnerTickable);
         
            SystemBus.Reset();
            SkillFactory.Initialize(skillTickable, new SkillQuery(agentTickable));
            
            spawnerTickable.Initialize(1, request =>
            {
                float2 position = request.Position;
                float radius = request.Radius;
                
                int entity = EntityManager.CreateEntity();
                int monsterId = request.MonsterID;
                AgentData agentData = agentTickable.CreateAgent(entity, position, radius);
                
                ComponentManager<HealthData>.Add(entity, new HealthData(10));
                ComponentManager<MonsterData>.Add(entity, new MonsterData(monsterId));
                entityToAgent[entity] = agentData.agent;
            });
            
            EntityManager.OnEntityRemoved += EntityRemoved;
        }

        private async void Start()
        {
            GameObject arcParent = new GameObject("ArcParent");
            arcParent.transform.SetParent(uiCanvas.transform);
            arcParent.transform.SetAsFirstSibling();
            arcParent.transform.localPosition = Vector3.zero;
            arcParent.transform.localScale = Vector3.one;
            for (int i = 0; i < totalDice; i++)
            {
                var arc = Instantiate(arcMovePrefab, arcParent.transform);
                arc.transform.localPosition = Vector3.zero;
                arc.transform.localScale = Vector3.one;
                listArcs.Add(arc);
            }

            // instance
            battleLevel = Instantiate(battleLevelPrefab, transform);
            await UniTask.WaitForSeconds(1);
            await battleLevel.Initialize(totalDice, timeScale);
            await UniTask.WaitForSeconds(0.35f);
            controlDice = Instantiate(controlDicePrefab, uiCanvas.transform);
            controlDice.transform.SetAsFirstSibling();
            controlDice.PrefabBuilder(totalDice, unlockAll);
            controlDice.OnDiceTrigger += OnDiceTrigger;
            await controlDice.Initialize();
            await UniTask.WaitForSeconds(0.2f);
            float f = battleLevel.Play();
            await UniTask.WaitForSeconds(f);
            controlDice.Play();
            tickSystemOwner.IsPaused = false;
            tickSystemOwner.TryGetTickable(out SpawnerTickable spawnerTickable);
            spawnerTickable.WaveSpawn();

            bool waveSpawnComplete = false;
            int totalEntityInScene = 0;

            void SpawnAction()
            {
                if (totalEntityInScene == 0 && waveSpawnComplete)
                {
                    this.WaitInvoke(2, () =>
                    {
                        bool spawn = spawnerTickable.WaveSpawn();
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
            spawnerTickable.OnWaveCompleted += () =>
            {
                waveSpawnComplete = true;
                SpawnAction();
            };
        }

        private void OnDiceTrigger(List<(int order, int number)> list)
        {
            // trigger: (0, 1),(1, 5),(2, 4),(3, 3)
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = 0;
                triggers[i] = false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                int ii = list[i].number - 1;
                numbers[ii]++;
            }

            int index = 0;
            int count = 0;
            int listCount = list.Count;
            for (int i = 0; i < listCount; i++)
            {
                GetNumberIndex(index, out int numberIndex, out int numberStack);
                Vector3 start = controlDice.GetUIDiceWorldPosition(i) + Vector3.up * 0.1f;
                //Debug.Log("Bay numberstack chưa đúng, ví dụ star đang hiện tại trên cone là 3, thì phải bay tới vị trí 3 thay vì 0");
                Vector3 end = battleLevel.GetConeWorldPosition(numberIndex, numberStack);
                Vector3 rot = battleLevel.GetConeWorldRotation(numberIndex, numberStack);
                listArcs[index].MoveTo(start, end, rot, numberStack * 0.2f, duration,
                    RandomUtils.Range(minRadius, maxRadius), 2.5f,
                    RandomUtils.Range(0.3f, 0.5f), () =>
                    {
                        count++;
                        if (!triggers[numberIndex])
                        {
                            triggers[numberIndex] = true;
                            battleLevel.ResetStack(numberIndex);
                        }
                        battleLevel.TriggerStack(numberIndex, numberStack + 1);
                        if (count == listCount) battleLevel.Trigger(numbers);
                    });
                index++;
            }
        }

        private void EntityRemoved(int entity)
        {
            if (entityToAgent.Remove(entity, out int agentId)) agentTickable.DestroyAgent(agentId);
        }

        // numbers: [1,2,0,0,1,0]
        private void GetNumberIndex(int index, out int numberIndex, out int numberStack)
        {
            numberIndex = numberStack = 0;
            int t = index;
            for (int i = 0; i < numbers.Length; i++)
            {
                var n = numbers[i];
                if (t < n)
                {
                    numberIndex = i;
                    numberStack = n - t - 1;
                    return;
                }

                t -= n;
            }
        }
    }
}*/