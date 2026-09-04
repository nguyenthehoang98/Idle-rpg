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
    ///
    /// Cách map SkillConfigData:
    /// - totalDuration = projectileDuration (đạn bay đúng thời gian này rồi tự huỷ)
    /// - collisionDelayInit / collisionDuration: cửa sổ bật/tắt detector va chạm
    /// - hitCount: giới hạn số target (min 1)
    /// - hitInterval > 0: DOT (tick damage lặp). = 0: hit 1 lần
    /// - parallelProjectileCount + projectileDistanceStep: count viên phụ dịch ngang đều 2 bên
    ///   (viên thứ i lệch (i+1)/2*step*±1), viên chính luôn bắn thẳng baseDir scale 1
    /// - spreadProjectileCount + projectileAngleStep: count viên phụ xoay 2 bên quanh viên chính,
    ///   viên phụ thứ i lệch (i+1)/2*(angleStep/2)*±1; thường chỉ 1 trong 2 loại > 0
    /// - spread/parallelDamageScale: hệ số damage viên phụ (viên chính luôn = 1)
    /// </summary>
    public static class SkillFactory
    {
        private static SkillProcessingUnit unit;
        private static readonly HashSet<string> registeredPools = new HashSet<string>();

        // bật vẽ ray debug hướng bay từng viên (Scene view, lúc Play). Tắt khi xác nhận xong.
        private const bool DebugRays = false;
        private const float DebugRayDuration = 3f;

        public static void Initialize(SkillProcessingUnit processingUnit)
        {
            unit = processingUnit;
        }

        public static void Dispose()
        {
            unit = null;
            registeredPools.Clear();
        }

        public static async UniTask CastSkillAsync(SkillConfigData skillConfig, Vector3 from, Monster target,
            Action<Monster, float> onDamage)
        {
            if (unit == null)
            {
                Debug.LogError("[SkillFactory] Not initialized.");
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

            // totalDuration = thời gian bay hết projectileDuration (đạn bay rồi tự huỷ)
            float totalDuration = Mathf.Max(0.01f, skillConfig.projectileDuration);

            int hitCount = Mathf.Max(1, skillConfig.hitCount);

            // ===== spawn thẳng tại target (PlaceProjectile): không bay, đặt ngay tại vị trí đích =====
            if (skillConfig.spawnAtTarget)
            {
                if (target == null)
                {
                    Debug.LogWarning($"[SkillFactory] skill {skillConfig.skillId} spawnAtTarget nhưng không có target");
                    return;
                }

                SpawnProjectile(skillConfig, prefab, target.transform.position, target.transform.position,
                    1f, totalDuration, hitCount, onDamage, spawnAtTarget: true);
                return;
            }

            // hướng gốc: từ hero tới target (hoặc hướng mặt nếu không có target)
            Vector3 baseDir = target != null
                ? (target.transform.position - from).normalized
                : Vector3.right;

            // ===== xây danh sách viên đạn =====
            // Ngữ nghĩa count = SỐ VIÊN PHỤ (mỗi viên +1 đối xứng quanh viên chính). Viên chính luôn bắn scale 1.
            // spread: xoay góc; parallel: dịch lane ngang. Thường chỉ 1 trong 2 > 0.
            int parallelCount = Mathf.Max(0, skillConfig.parallelProjectileCount);
            int spreadCount = Mathf.Max(0, skillConfig.spreadProjectileCount);

            // viên chính scale 1, thẳng baseDir, spawn tại `from`
            SpawnProjectile(skillConfig, prefab, from, baseDir, 1f,
                totalDuration, hitCount, onDamage);

            // debug: 1 ray / viên, đúng vị trí spawn + hướng, dài = quãng đường bay được
            float flyDist = skillConfig.projectileSpeed * totalDuration;
            if (DebugRays) Debug.DrawRay(from, baseDir * flyDist, Color.green, DebugRayDuration);

            // viên phụ: (offset tương đối so với `from`, hướng, damageScale)
            var shots = new List<(Vector3 offset, Vector3 dir, float scale)>();
            Vector3 perp = new Vector3(-baseDir.y, baseDir.x, 0);

            // parallel: n viên phụ dịch ngang 2 bên, cùng hướng baseDir. n chẵn -> lane cân đối quanh trục;
            // n lẻ -> 1 viên giữa (không trùng viên chính vì nó nằm lane lệch ±, không phải lane 0).
            if (parallelCount > 0)
            {
                float space = Mathf.Max(0.05f, skillConfig.projectileDistanceStep);
                float scale = skillConfig.parallelDamageScale > 0 ? skillConfig.parallelDamageScale : 1f;

                for (int i = 1; i <= parallelCount; i++)
                {
                    float side = (i % 2 == 1) ? 1f : -1f;
                    int level = (i + 1) / 2;
                    float off = side * level * space;
                    if (DebugRays) Debug.DrawRay(from + perp * off, baseDir * flyDist, Color.cyan, DebugRayDuration);
                    shots.Add((perp * off, baseDir, scale));
                }
            }

            // spread: n viên phụ xoay 2 bên quanh viên chính. Lần 1 = +angleStep/2, lần 2 = -angleStep/2,
            // lần 3 = +angleStep, lần 4 = -angleStep... tức viên phụ thứ i lệch = (i+1)/2 * angleStep/2 * ±1.
            if (spreadCount > 0)
            {
                float halfStep = Mathf.Max(0, skillConfig.projectileAngleStep) * 0.5f;
                float scale = skillConfig.spreadDamageScale > 0 ? skillConfig.spreadDamageScale : 1f;

                for (int i = 1; i <= spreadCount; i++)
                {
                    float side = (i % 2 == 1) ? 1f : -1f;
                    int level = (i + 1) / 2;
                    float angle = side * level * halfStep;
                    Vector3 dir = Quaternion.Euler(0, 0, angle) * baseDir;
                    if (DebugRays) Debug.DrawRay(from, dir * flyDist, Color.magenta, DebugRayDuration);
                    shots.Add((Vector3.zero, dir, scale));
                }
            }

            foreach (var shot in shots)
            {
                SpawnProjectile(skillConfig, prefab, from + shot.offset, shot.dir, shot.scale,
                    totalDuration, hitCount, onDamage);
            }
        }

        private static void SpawnProjectile(SkillConfigData skillConfig, GameObject prefab,
            Vector3 origin, Vector3 dir, float damageScale,
            float totalDuration, int hitCount,
            Action<Monster, float> onDamage, bool spawnAtTarget = false)
        {
            GameObject go = Pool.Instantiate(prefab, origin, true);
            go.transform.rotation = Quaternion.identity;

            Projectile projectile = go.GetComponent<Projectile>();
            if (projectile == null)
            {
                Debug.LogError($"[SkillFactory] Prefab '{skillConfig.prefabName}' missing Projectile component");
                Pool.Destroy(go);
                return;
            }

            if (spawnAtTarget)
            {
                // sinh thẳng tại chỗ (đích = vị trí spawn), không bay
                projectile.SetupAt(origin, dir, totalDuration);
            }
            else
            {
                projectile.Setup(origin, dir, skillConfig.projectileSpeed, totalDuration);
            }

            CollisionDetector[] detectors = go.GetComponentsInChildren<CollisionDetector>();
            if (detectors.Length == 0)
            {
                Debug.LogError($"[SkillFactory] Prefab '{skillConfig.prefabName}' has no CollisionDetector");
                return;
            }

            float collisionDelayInit = Mathf.Max(0, skillConfig.collisionDelayInit);
            float collisionDuration = Mathf.Max(0, skillConfig.collisionDuration);

            // DOT: damage lặp theo chu kỳ. hitInterval>0 là chu kỳ chính (spec);
            // fallback damageTickInterval nếu chỉ có cái đó.
            bool isDot = skillConfig.hitInterval > 0 || skillConfig.damageTickInterval > 0;
            float dotInterval = skillConfig.hitInterval > 0
                ? skillConfig.hitInterval
                : skillConfig.damageTickInterval;

            ProjectileSkillAction action = new ProjectileSkillAction(
                totalDuration, detectors,
                dotInterval, skillConfig.hitInterval, hitCount,
                collisionDelayInit, collisionDuration
            );
            HashSet<Monster> modifierTargets = new HashSet<Monster>();
            action.OnComplete += modifierTargets.Clear;

            action.OnDamaged += (HitInfo info) =>
            {
                Monster m = info.Unique as Monster;
                if (m == null) return false;

                float dmg = damageScale; // base damage; Hero nhân với attack stat

                onDamage?.Invoke(m, dmg);
                if (modifierTargets.Add(m)) ApplyModifier(skillConfig, m);

                if (isDot)
                {
                    // overlap chỉ đăng ký target + damage đầu; damage lặp do ticker
                    // trả false để không đếm totalHit / không Interrupt
                    projectile.StopMotion();
                    return false;
                }

                // single-hit: trúng là hết
                if (info.IsLastHit) projectile.DestroySelf();

                return true;
            };

            unit.RequestAddAction(skillConfig.skillId, action);
        }

        private static void ApplyModifier(SkillConfigData skillConfig, Monster target)
        {
            if (target == null || target.CurrentHealth <= 0) return;
            if (!SkillModifierSelector.TrySelect(
                    skillConfig.modifiers,
                    UnityEngine.Random.value,
                    UnityEngine.Random.value,
                    out SkillModifierData modifier)) return;

            float lifeTime = Mathf.Max(0.01f, modifier.duration);
            float tickInterval = Mathf.Max(0.01f, modifier.tickInterval);
            float tickElapsed = 0f;

            ModifierSkillAction action = new ModifierSkillAction(
                lifeTime,
                modifierAction => target.ApplyModifier(modifierAction, modifier),
                (modifierAction, deltaTime) =>
                {
                    if (target == null || target.CurrentHealth <= 0)
                    {
                        modifierAction.Interrupt();
                        return;
                    }

                    if (modifier.type != SkillModifierType.Bleed) return;

                    tickElapsed += deltaTime;
                    while (tickElapsed >= tickInterval)
                    {
                        tickElapsed -= tickInterval;
                        target.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(modifier.value)));

                        if (target.CurrentHealth <= 0)
                        {
                            modifierAction.Interrupt();
                            break;
                        }
                    }
                },
                modifierAction => target.RemoveModifier(modifierAction));

            unit.RequestAddAction(skillConfig.skillId, action);
        }
    }
}
