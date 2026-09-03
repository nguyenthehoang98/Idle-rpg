using System;
using System.Collections.Generic;
using _GameToolkit.Colliders;
using _GameToolkit.ResourceManagement;
using _GameToolkit.Share;
using _GameToolkit.Skills;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.Battle
{
    /// <summary>
    /// Tạo + chạy skill (projectile). Mỗi phát bắn = 1 ProjectileSkillAction
    /// được xử lý trong SkillProcessingUnit (tự quản vòng tick).
    /// </summary>
    public static class SkillFactory
    {
        private static SkillProcessingUnit unit;
        private static readonly HashSet<string> registeredPools = new HashSet<string>();

        /// <summary>Gán unit xử lý chung (lấy từ SkillTickRunner ngoài scene).</summary>
        public static void Initialize(SkillProcessingUnit processingUnit)
        {
            unit = processingUnit;
        }

        public static void Dispose()
        {
            unit = null;
            registeredPools.Clear();
        }

        /// <summary>
        /// Bắn 1 projectile từ from (hero) tới target.
        /// onDamage(gây bao nhiêu) được gọi khi trúng target.
        /// </summary>
        public static async UniTask CastSkillAsync(SkillConfigData skillConfig, Vector3 from, Monster target,
            Action<float> onDamage)
        {
            if (unit == null)
            {
                Debug.LogError("[SkillFactory] Not initialized. Call SkillFactory.Initialize(unit) first.");
                return;
            }

            if (string.IsNullOrEmpty(skillConfig.prefabName))
            {
                Debug.LogError("[SkillFactory] skillConfig.prefabName is empty");
                return;
            }

            GameObject prefab = await AssetLoader.GetAssetCached<GameObject>(skillConfig.prefabName);

            if (prefab == null)
            {
                Debug.LogError($"[SkillFactory] Projectile prefab '{skillConfig.prefabName}' not found");
                return;
            }

            if (registeredPools.Add(skillConfig.prefabName))
            {
                Pool.RegisterPool(prefab, true);
            }

            // 1) spawn projectile, bắt đầu bay
            GameObject go = Pool.Instantiate(prefab, from, true);
            go.transform.rotation = Quaternion.identity;

            Projectile projectile = go.GetComponent<Projectile>();
            if (projectile == null)
            {
                Debug.LogError($"[SkillFactory] Prefab '{skillConfig.prefabName}' missing Projectile component");
                return;
            }

            Vector3 to = target != null ? target.transform.position : from + Vector3.right;
            Vector3 dir = (to - from);
            float distance = dir.magnitude;
            dir.Normalize();

            projectile.Setup(from, to, skillConfig.projectileSpeed);

            // 2) detector(s) để biết projectile trúng monster nào
            CollisionDetector[] detectors = go.GetComponentsInChildren<CollisionDetector>();
            if (detectors.Length == 0)
            {
                Debug.LogError($"[SkillFactory] Prefab '{skillConfig.prefabName}' has no CollisionDetector");
                return;
            }

            // lifetime = thời gian bay hết quãng đường
            float lifeTime = distance / Mathf.Max(0.01f, skillConfig.projectileSpeed);

            // hitCount: số target tối đa 1 phát trúng (mặc định 1 nếu config 0)
            int hitCount = skillConfig.hitCount > 0 ? skillConfig.hitCount : 1;

            ProjectileSkillAction action = new ProjectileSkillAction(
                lifeTime, detectors,
                skillConfig.damageTickInterval, skillConfig.hitInterval, hitCount
            );

            action.OnDamaged += (HitInfo info) =>
            {
                // chỉ trừ máu đúng target được chọn (tránh bắn trúng quái khác)
                if (info.Unique != target) return false;

                onDamage?.Invoke(10f); // tạm: damage hardcode, sẽ tính theo stat Hero

                projectile.DestroySelf();

                return true;
            };

            // 3) đăng ký vào unit -> Startup() gọi detector.Startup() bắt đầu overlap
            unit.RequestAddAction(skillConfig.skillId, action);
        }
    }
}
