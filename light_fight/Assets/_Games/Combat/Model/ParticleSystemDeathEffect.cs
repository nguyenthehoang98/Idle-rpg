using System;
using _KIT.Pool;
using _KIT.Utils;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class ParticleSystemDeathEffect : BaseDeathEffect
    {
        [SerializeField] private ParticleSystem deathEffectPrefab;

        private void Awake()
        {
            KitPool.RegisterPool(deathEffectPrefab.gameObject, true);
        }

        public override void Play(Action onComplete)
        {
            GameObject go = Instantiate(deathEffectPrefab.gameObject);
            go.transform.position = transform.position;
            if (earlyPlayTime > 0)
            {
                this.WaitInvoke(earlyPlayTime, () => { visual.gameObject.SetActive(false); });
            }
            else
            {
                visual.gameObject.SetActive(false);                
            }

            this.WaitInvoke(deathEffectPrefab.main.duration, () =>
            {
                visual.gameObject.SetActive(true);
                KitPool.Destroy(go);
                onComplete();
            });
        }
    }
}