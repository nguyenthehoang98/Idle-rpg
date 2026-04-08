using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _KIT.Event;
using _KIT.Resource;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    
    private Entity entity;
    
    public static async void Instantiate(Transform parent, Entity entity)
    {
        GameObject go = await KitLoaded.LoadAsync<GameObject>("PlayerHealthUI");
        PlayerHealthUI instance = Instantiate(go, parent).GetComponent<PlayerHealthUI>();
        instance.entity = entity;
        instance.UpdateHealth();
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        EventBus.Instance.Subscribe<PlayerOnDamageEvent>(OnPlayerOnDamage);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PlayerOnDamageEvent>(OnPlayerOnDamage);
    }
    
    private void OnPlayerOnDamage(PlayerOnDamageEvent e)
    {
        UpdateHealth();
    }

    void UpdateHealth()
    {
        var component = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<HealthData>(entity);
        healthFill.fillAmount = component.Health / (float)component.MaxHealth;
        healthText.SetText(Mathf.Max(component.Health, 0).ToString());

        if (component.Health <= 0)
        {
            EventBus.Instance.Publish(new WaveLoseEvent());
        }
    }
}
