using System.Collections.Generic;
using _Game.Battle.Ecs.Events;
using _KIT.Event;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.Ecs.View
{
    public class DamageTextManager : MonoBehaviour
    {
        [SerializeField] private DamageText prefab;

        public static DamageTextManager Instance { get; private set; }

        private readonly Queue<DamageText> pool = new Queue<DamageText>();

        private void Awake()
        {
            Instance = this;
        }
    
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
            Spawn(e.Position, e.Damage);
        }

        private DamageText CreateNew()
        {
            var item = Instantiate(prefab, transform);
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
            return item;
        }

        private void Spawn(float2 worldPos, int damage,
            DamageType type = DamageType.Normal)
        {
            var item = pool.Count > 0 ? pool.Dequeue() : CreateNew();

            item.transform.SetParent(transform);
            item.transform.position = new Vector3(worldPos.x, worldPos.y, 0);

            var style = GetStyle(type);

            item.Init(damage.ToString(),
                style.color, style.scale
            );
        }

        public void Release(DamageText item)
        {
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }
    
        private DamageStyle GetStyle(DamageType type)
        {
            return type switch
            {
                DamageType.Crit => new DamageStyle(Color.yellow, 1.4f),
                DamageType.Poison => new DamageStyle(Color.green, 0.9f),
                DamageType.Burn => new DamageStyle(new Color(1f, 0.4f, 0f), 1f),
                _ => new DamageStyle(Color.white, 1f)
            };
        }
    }

    public enum DamageType
    {
        Normal,
        Crit,
        Poison,
        Burn
    }

    public struct DamageStyle
    {
        public Color color;
        public float scale;

        public DamageStyle(Color c, float s)
        {
            color = c;
            scale = s;
        }
    }
}