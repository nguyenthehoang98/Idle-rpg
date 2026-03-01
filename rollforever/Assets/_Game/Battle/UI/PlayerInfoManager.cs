using System;
using _Game.Battle.Ecs.Events;
using _KIT.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Battle.UI
{
    public class PlayerInfoManager : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private Image healthFill;
        [SerializeField] private TextMeshProUGUI textHealth;
        
        private int shield;
        private int health;

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<DamagePlayerEvent>(OnDamagePlayer);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<DamagePlayerEvent>(OnDamagePlayer);
        }

        private void OnDamagePlayer(DamagePlayerEvent e)
        {
            healthFill.fillAmount = e.CurrentHealth / (float)e.MaxHealth;
            textHealth.SetText(e.CurrentHealth.ToString());
        }
    }
}
