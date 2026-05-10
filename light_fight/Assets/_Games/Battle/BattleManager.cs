using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private BattleLevel battleLevel;
        [SerializeField] private UIBattleControlArena correctArena;

        private async void Start()
        {
            await UniTask.WaitForSeconds(1);
            await battleLevel.Initialize();
            await UniTask.WaitForSeconds(0.35f);
            await correctArena.Initialize();
            await UniTask.WaitForSeconds(0.2f);
            battleLevel.Play();
            await UniTask.WaitForSeconds(0.2f);
            correctArena.Play();
        }
    }
}