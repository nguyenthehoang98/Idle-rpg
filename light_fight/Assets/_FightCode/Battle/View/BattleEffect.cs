using System;
using _FightCode.Utils;
using _KITSystem.Resource;
using _KITSystem.Utils;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _FightCode.Battle.View
{
    public class BattleEffect : Singleton<BattleEffect>
    {
        [SerializeField] private MMF_Player hitEffectPrefab;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if(arg0.name == Const.BATTLE_SCENE)
            {
                KitPool.RegisterPool(hitEffectPrefab.gameObject, true);
            }
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