using System;
using _Games.GamePlay.AnimationSystem;
using _Games.GamePlay.SpawnerSystem;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using UnityEngine;

namespace _Games.GamePlay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TickSystemOwner owner;
        [SerializeField] private Pedestal pedestal;
        [SerializeField] private Character[] characters;

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

        private void Start()
        {
            AssetBundleManager.SetLocationBundle(true);
            owner.Initialize();
        }

        private void Update()
        {
            float duration = 0.5f;

            if (Input.GetKeyDown(KeyCode.Alpha1)) HeroActivate(0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha2)) HeroActivate(1, duration);
            if (Input.GetKeyDown(KeyCode.Alpha3)) HeroActivate(2, duration);
            if (Input.GetKeyDown(KeyCode.Alpha4)) HeroActivate(3, duration);
            if (Input.GetKeyDown(KeyCode.Alpha5)) HeroActivate(4, duration);
            if (Input.GetKeyDown(KeyCode.Alpha6)) HeroActivate(5, duration);
            if (Input.GetKeyDown(KeyCode.Alpha7)) HeroActivate(6, duration);

            if (Input.GetKeyDown(KeyCode.F1)) HeroDeactivate(0, duration);
            if (Input.GetKeyDown(KeyCode.F2)) HeroDeactivate(1, duration);
            if (Input.GetKeyDown(KeyCode.F3)) HeroDeactivate(2, duration);
            if (Input.GetKeyDown(KeyCode.F4)) HeroDeactivate(3, duration);
            if (Input.GetKeyDown(KeyCode.F5)) HeroDeactivate(4, duration);
            if (Input.GetKeyDown(KeyCode.F6)) HeroDeactivate(5, duration);
            if (Input.GetKeyDown(KeyCode.F7)) HeroDeactivate(6, duration);
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
        
        private void HeroActivate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;

            pedestal.Activate(slotIndex, duration);

            if (slotIndex > 5) return;
            var character = characters[slotIndex];
            if (character != null) character.Activate(duration);
        }

        private void HeroDeactivate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;
            
            pedestal.Deactivate(slotIndex, duration);

            if (slotIndex > 5) return;
            var character = characters[slotIndex];
            if (character != null) character.Deactivate(duration);
        }
    }
}