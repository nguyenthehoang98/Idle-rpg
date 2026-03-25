using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _KIT.Event;
using PrimeTween;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    
    private Entity entity;

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        EventBus.Instance.Subscribe<PlayerOnDamageEvent>(OnPlayerOnDamage);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PlayerOnDamageEvent>(OnPlayerOnDamage);
    }

    public void Initialize(Entity entity)
    {
        this.entity = entity;
        UpdateHealth();
    }

    private void OnPlayerOnDamage(PlayerOnDamageEvent e)
    {
        UpdateHealth();
    }

    void UpdateHealth()
    {
        var component = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<HealthData>(entity);
        healthFill.fillAmount = component.Health / (float)component.MaxHealth;
        healthText.SetText(component.Health.ToString());
    }
}
