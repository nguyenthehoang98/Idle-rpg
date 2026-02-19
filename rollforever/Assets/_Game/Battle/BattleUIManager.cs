using System;
using System.Linq;
using _Game.Battle.Events;
using _KIT.Event;
using _KIT.Schedule;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Battle
{
    public class BattleUIManager : MonoBehaviour
    {
        [SerializeField] private GameLoop gameLoop;
        [SerializeField] private GameObject container;
        [SerializeField] private Button buttonNextWave;

        private Action onNextWave;
        
        private void Awake()
        {
            buttonNextWave.onClick.AddListener(() =>
            {
                onNextWave?.Invoke();
                onNextWave = null;
                gameLoop.Resume();
                container.SetActive(false);
            });
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<NextWaveEvent>(OnNextWave);
        }
        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<NextWaveEvent>(OnNextWave);
        }

        private void OnNextWave(NextWaveEvent e)
        {
            gameLoop.Pause();
            Systems.AbilitySystem system =
                e.Systems.GetAllSystems().FirstOrDefault(system => system.GetType() == typeof(Systems.AbilitySystem)) as
                    Systems.AbilitySystem;
            system.ClearAll();
            onNextWave = e.OnCompleted;
            container.gameObject.SetActive(true);
        }
    }
}
