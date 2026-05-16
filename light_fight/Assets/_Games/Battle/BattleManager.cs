using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private float timeScale = 1;
        [SerializeField] private bool unlockAll;
        [SerializeField, Range(1, 4)] private int totalDice = 2;
        [SerializeField] private BattleLevel battleLevel;
        [SerializeField] private UIBattleControlArena correctArena;

        private void Awake()
        {
            correctArena.PrefabBuilder(totalDice, unlockAll);
            correctArena.OnTrigger += battleLevel.Trigger;
        }

        private async void Start()
        {
            Debug.Log(@"Khi dice chuyển sang 1 number khác thì có hệu ưnứng như mấy trò gacha ý, Hiệu ứng scroll jackpot spin");
            await UniTask.WaitForSeconds(1);
            await battleLevel.Initialize(timeScale);
            await UniTask.WaitForSeconds(0.35f);
            await correctArena.Initialize();
            await UniTask.WaitForSeconds(0.2f);
            float f = battleLevel.Play();
            await UniTask.WaitForSeconds(f);
            correctArena.Play();
        }
    }
}