using _Games.GamePlay.AnimationSystem;
using _Games.GamePlay.SpawnerSystem;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using UnityEngine;

namespace _Games.GamePlay
{
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TickSystemOwner owner;
        [SerializeField] private Pedestal pedestal;

        private SpawnerTickable spawner;
        private int totalMonsterAlive = 0;

        private void Awake()
        {
            owner.TryGetTickable(out spawner);
            spawner.OnWaveSpawnCompleted += waveIndex =>
            {
                Debug.Log($"Complete wave {waveIndex} - {spawner.IsCompleted}");
            };

            Monster.OnMonsterEnable += MonsterEnable;
            Monster.OnMonsterDisable += MonsterDisable;
        }

        private async void Start()
        {
            AssetBundleManager.SetLocationBundle(true);

            await owner.Initialize();
        }

        private void Update()
        {
            float duration = 0.2f;

            if (Input.GetKeyDown(KeyCode.F1)) SetWeapon(0, 1, duration);
            if (Input.GetKeyDown(KeyCode.F2)) SetWeapon(0, 2, duration);
            if (Input.GetKeyDown(KeyCode.F3)) SetWeapon(0, 3, duration);

            if (Input.GetKeyDown(KeyCode.F4)) SetWeapon(1, 1, duration);
            if (Input.GetKeyDown(KeyCode.F5)) SetWeapon(1, 2, duration);
            if (Input.GetKeyDown(KeyCode.F6)) SetWeapon(1, 3, duration);

            if (Input.GetKeyDown(KeyCode.F7)) SetWeapon(2, 1, duration);
            if (Input.GetKeyDown(KeyCode.F8)) SetWeapon(2, 2, duration);
            if (Input.GetKeyDown(KeyCode.F9)) SetWeapon(2, 3, duration);

            if (Input.GetKeyDown(KeyCode.F10)) SetWeapon(3, 1, duration);
            if (Input.GetKeyDown(KeyCode.F11)) SetWeapon(3, 2, duration);
            if (Input.GetKeyDown(KeyCode.F12)) SetWeapon(3, 3, duration);

            if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeapon(0, 0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeapon(1, 0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetWeapon(2, 0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetWeapon(3, 0, duration);
        }

        private void OnDestroy()
        {
            Monster.OnMonsterEnable -= MonsterEnable;
            Monster.OnMonsterDisable -= MonsterDisable;
        }

        private void MonsterDisable(Monster monster)
        {
            totalMonsterAlive--;

            if (totalMonsterAlive == 0 && spawner.IsPaused)
            {
                spawner.IsPaused = false;
            }
        }

        private void MonsterEnable(Monster monster)
        {
            totalMonsterAlive++;
        }

        private void SetWeapon(int slot, int level, float duration)
        {
            if (slot < 0 || slot > 4) return;

            pedestal.SetWeapon(slot, level, duration, Time.deltaTime);
        }
    }
}