using System.Collections.Generic;
using _Game.Battle.Ecs.Events;
using _KIT.Event;
using TMPro;
using UnityEngine;

namespace _Game.Battle.UI
{
    public class ReportManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textPrefab;

        Dictionary<int, TextMeshProUGUI> texts = new Dictionary<int, TextMeshProUGUI>();
        Dictionary<int, int> damageTaken = new Dictionary<int, int>();
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<DamageMonsterEvent>(OnDamageMonster);
        }

        private void OnDisable()
        {
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
    }
}
