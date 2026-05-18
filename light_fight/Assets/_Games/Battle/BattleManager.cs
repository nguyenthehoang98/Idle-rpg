using System.Collections.Generic;
using _KITSystem.EventBus;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [TitleGroup("Settings")]
        ///
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

        private UIBattleControlDiceSpeed controlDice;
        private BattleLevel battleLevel;
        private SpawnerTickable spawner;
        private List<ArcMove> listArcs = new List<ArcMove>();
        private readonly int[] numbers = new int[BattleConst.MAX_DICE_NUMBER];
        
        private void Awake()
        {
            Debug.Log(@"UIBattleDiceSlot nên dùng queue để tính number dice. có thể config độ khó theo level theo các trường\n
- tỉ lệ quay vào ô chứa trang bị\n
	+ Tỉ lệ lặp lại ô chứa trang bị theo level, power. level max =50% chả hạn\n
- tỉ lệ quay vào ô không chứa trang bị");
            
            // global
            SystemBus.Init();
        }

        private void OnEnable()
        {
            SystemBus.Subscribe<QueryAgentSignal>(OnQueryAgent);
        }

        private void OnQueryAgent(QueryAgentSignal signal)
        {
            if (spawner == null) tickSystemOwner.TryGetTickable(out spawner);
            if (spawner != null)
            {
                int count = spawner.Query(signal.Position, signal.Radius, out var results);
                signal.OnQueryAgent?.Invoke((count, results));
            }
        }

        private void OnDisable()
        {
            SystemBus.Unsubscribe<QueryAgentSignal>(OnQueryAgent);
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
        }

        private void OnDiceTrigger(List<(int order, int number)> list)
        {
            // trigger: (0, 1),(1, 5),(2, 4),(3, 3)
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = 0;
            }

            for (int i = 0; i < list.Count; i++)
            {
                int ii = list[i].number - 1;
                numbers[ii]++;
            }

            int index = 0;
            int count = 0;
            int listCount = list.Count;
            for (int i = 0; i < numbers.Length; i++)
            {
                int value = numbers[i];
                if (value > 0)
                {
                    Vector3 start = controlDice.GetUIDiceWorldPosition(index) + Vector3.up * 0.1f;
                    for (int i1 = 0; i1 < value; i1++)
                    {
                        Vector3 end = battleLevel.GetConeWorldPosition(i, i1);
                        Vector3 rot = battleLevel.GetConeWorldRotation(i, i1);
                        listArcs[index].MoveTo(
                            start, end, rot, duration,
                            RandomUtils.Range(minRadius, maxRadius),
                            2.5f,
                            RandomUtils.Range(0.3f, 0.5f),
                            () =>
                            {
                                count++;
                                if (count == listCount) battleLevel.Trigger(numbers);
                            }
                        );
                    }
                    
                    index++;
                }
            }
        }
    }
}