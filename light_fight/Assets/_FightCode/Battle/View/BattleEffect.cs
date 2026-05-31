using _KITSystem.Resource;
using _KITSystem.Utils;
using MoreMountains.Feedbacks;
using UnityEngine;
namespace _FightCode.Battle.View
{
    public class BattleEffect : Singleton<BattleEffect>
    {
        [SerializeField] private MMF_Player hitEffectPrefab;

        protected override void Awake()
        {
            base.Awake();
            KitPool.RegisterPool(hitEffectPrefab.gameObject, true);
        }

        public MMF_Player SpawnHitEffect(Vector3 position)
        {
            MMF_Player mmf = KitPool.Instantiate(hitEffectPrefab.gameObject).GetComponent<MMF_Player>();
            mmf.transform.position = position;
            mmf.PlayFeedbacks();
            this.WaitInvoke(mmf.TotalDuration, () =>
            {
                if (mmf != null && mmf.IsPlaying)
                {
                    mmf.StopFeedbacks();
                    KitPool.Destroy(mmf.gameObject);
                }
            });
            return mmf;
        }
    }
}