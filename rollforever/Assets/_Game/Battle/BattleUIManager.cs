using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Battle.Events;
using _KIT.Event;
using _KIT.Schedule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Battle
{
    public class BattleUIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textPrefab;
        [SerializeField] private GameLoop gameLoop;
        [SerializeField] private Button buttonNextWave;

        Dictionary<int, TextMeshProUGUI> texts = new Dictionary<int, TextMeshProUGUI>();
        Dictionary<int, int> damageTaken = new Dictionary<int, int>();
        private Action onNextWave;
        
        private void Awake()
        {
            buttonNextWave.onClick.AddListener(() =>
            {
                onNextWave?.Invoke();
                onNextWave = null;
                gameLoop.Resume();
                buttonNextWave.gameObject.SetActive(false);
            });
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<NextWaveEvent>(OnNextWave);
            EventBus.Instance.Subscribe<DamageMonsterEvent>(OnDamageMonster);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<NextWaveEvent>(OnNextWave);
            EventBus.Instance.Unsubscribe<DamageMonsterEvent>(OnDamageMonster);
        }

        private void OnDamageMonster(DamageMonsterEvent e)
        {
            if (!texts.TryGetValue(e.Source, out TextMeshProUGUI text))
            {
                text = Instantiate(textPrefab, textPrefab.transform.parent);
                texts.Add(e.Source, text);
                damageTaken.Add(e.Source, 0);
            }

            damageTaken[e.Source] += e.Damage;
            texts[e.Source].SetText($"[{e.Source}] {damageTaken[e.Source]}");
        }

        private void OnNextWave(NextWaveEvent e)
        {
            gameLoop.Pause();
            Systems.AbilitySystem system =
                e.Systems.GetAllSystems().FirstOrDefault(system => system.GetType() == typeof(Systems.AbilitySystem)) as
                    Systems.AbilitySystem;
            system.ClearAll();
            onNextWave = e.OnCompleted;
            buttonNextWave.gameObject.SetActive(true);
        }
    }
}
