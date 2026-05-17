using System;
using _KITSystem.EventBus;
using _KITSystem.Schedule;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [TitleGroup("Settings")]
        [SerializeField] private float timeScale = 1;
        [SerializeField] private bool unlockAll;
        [SerializeField, Range(1, 4)] private int totalDice = 2;
        
        [TitleGroup("Prefabs")]
        [SerializeField] private BattleLevel battleLevelPrefab;
        [SerializeField] private UIBattleControlDiceSpeed controlDicePrefab;
        [SerializeField] private UIBattleSpawnDiceText spawnDiceTextPrefab;

        [TitleGroup("Elements")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private TickSystemOwner tickSystemOwner;

        private SpawnerTickable spawner;
        
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
            int count = spawner.Query(signal.Position, signal.Radius, out var results);
            signal.OnQueryAgent?.Invoke((count, results));
        }

        private void OnDisable()
        {
            SystemBus.Unsubscribe<QueryAgentSignal>(OnQueryAgent);
        }

        private async void Start()
        {
            // instance
            var battleLevel = Instantiate(battleLevelPrefab, transform);
            await UniTask.WaitForSeconds(1);
            await battleLevel.Initialize(timeScale);
            await UniTask.WaitForSeconds(0.35f);
            var spawnDiceText = Instantiate(spawnDiceTextPrefab, uiCanvas.transform);
            spawnDiceText.transform.SetAsFirstSibling();
            var controlDice = Instantiate(controlDicePrefab, uiCanvas.transform);
            controlDice.transform.SetAsFirstSibling();
            controlDice.PrefabBuilder(totalDice, unlockAll);
            controlDice.OnTrigger += spawnDiceText.Spawn;
            controlDice.OnTrigger += battleLevel.Trigger;
            await controlDice.Initialize();
            await UniTask.WaitForSeconds(0.2f);
            float f = battleLevel.Play();
            await UniTask.WaitForSeconds(f);
            controlDice.Play();
            tickSystemOwner.IsPaused = false;
        }
    }
}