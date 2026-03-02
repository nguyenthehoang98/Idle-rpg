using _Game.Battle.Ecs.Events;
using _KIT.Event;
using _KIT.Utils;
using DG.Tweening;
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
        [Header("Tweens")]
        [SerializeField] private float yStartPosition;
        [SerializeField] private float closeDuration = 0.3f;
        
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

        public void Push()
        {
            float duration = closeDuration;
            content.gameObject.SetActive(true);
            Vector3 anchoredPosition3D = content.anchoredPosition3D;
            anchoredPosition3D.y = yStartPosition;
            content.anchoredPosition3D = anchoredPosition3D;
            content.DOAnchorPosY(0, duration).SetEase(Ease.OutSine);
        }

        public void Pull()
        {
            float duration = closeDuration;
            content.DOAnchorPosY(yStartPosition, duration).SetEase(Ease.OutSine);
            this.WaitInvoke(duration, () =>
            {
                content.gameObject.SetActive(false);
            });
        }
    }
}
