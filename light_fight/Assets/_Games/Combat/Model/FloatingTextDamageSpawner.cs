using UnityEngine;
using System;
using System.Collections.Generic;
using _Games.Combat.Event;
using _Games.Utils;
using _KIT.Event;
using _KIT.Resource;
using MoreMountains.Feedbacks;

namespace _Games.Combat.Model
{
    public class FloatingTextDamageSpawner : MonoBehaviour
    {
        [SerializeField] private Data[] allData;

        public static async void Instantiate(Transform parent)
        {
            GameObject go = await KitLoaded.LoadAsync<GameObject>(GlobalsPath.FloatingTextDamageSpawner);
            Instantiate(go, parent).GetComponent<FloatingTextDamageSpawner>();
        }
        
        Dictionary<TextDamageType, MMFloatingTextSpawner> dictionary = new Dictionary<TextDamageType, MMFloatingTextSpawner>();

        private void Awake()
        {
            foreach (var data in allData)
            {
                dictionary[data.type] = data.spawner;
            }
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<SpawnTextDamageEvent>(OnSpawnTextDamage);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<SpawnTextDamageEvent>(OnSpawnTextDamage);
        }

        private void OnSpawnTextDamage(SpawnTextDamageEvent e)
        {
            dictionary[e.Type].Spawn(e.Damage.ToString(), e.Position, Vector3.up);
        }

        [Serializable]
        class Data
        {
            public TextDamageType type;
            public MMFloatingTextSpawner spawner;
        }
    }

    public enum TextDamageType
    {
        Normal,
    }
}