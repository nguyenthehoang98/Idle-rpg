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

        private async void Start()
        {
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
        }
    }
}