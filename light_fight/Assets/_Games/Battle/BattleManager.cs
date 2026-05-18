using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private float timeScale = 1;
        [SerializeField] private BattleLevel battleLevel;
        [SerializeField] private UIBattleControlArena correctArena;

        private async void Start()
        {
            Debug.LogError(@"Khi slot zoom to ra bắt đầu vũ khí mới hoạt động, nó thò ra 1 chút rồi xoay xoay");
            Debug.LogError(@"Khi slot zoom nhỏ vào thì vũ khí quay về vị trí ngủ rồi mới zoom nhỏ lại");
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