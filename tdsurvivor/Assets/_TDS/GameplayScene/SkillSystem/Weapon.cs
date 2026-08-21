using System;
using System.Collections;
using System.Collections.Generic;
using _TDS.Config;
using _TDS.GameplayScene.Unit;
using _Toolkit.Avoidance;
using _Toolkit.SkillSystem.Core;
using UnityEngine;

namespace _TDS.GameplayScene.SkillSystem
{
    /// <summary>
    /// Lớp nền quản lý toàn bộ vòng đời của một vũ khí:
    /// khởi tạo → auto attack (cooldown → find target → rotate → play attack →
    /// execute → stop) → pause/time scale → outline theo cấp độ.
    ///
    /// Các lớp vũ khí cụ thể kế thừa và override các hàm virtual ở cuối file
    /// để tùy biến hành vi mà không cần chạm vào vòng lặp chính.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Weapon : MonoBehaviour
    {
        /* =====================================================================================
         * Projectile – kết nối với SkillManager (BuildSkill_Private đọc Equipment.projectile)
         * ===================================================================================== */
        public Projectile projectile;

        /* =====================================================================================
         * 0. Config base
         * ===================================================================================== */
        [Header("Owner")]
        [SerializeField] protected HeroController owner; // SC04 - force attack direction

        [Header("Find Target")]
        [SerializeField] protected FindTargetType findTargetType = FindTargetType.Nearest;
        [SerializeField] protected float attackRange = 3f;
        [SerializeField] protected float attackCooldown = 1f;
        [SerializeField] protected float retryFindTargetDelay = 0.2f;

        [Header("Damage / Crit")]
        [SerializeField] protected int attack = 10;
        [SerializeField] protected float critRate = 0f;
        [SerializeField] protected float critDamage = 1.5f;

        [Header("Projectile (khi projectile == null sẽ load theo asset name)")]
        [SerializeField] protected string projectileAssetName;
        [SerializeField] protected ProjectileContext projectileContext = new ProjectileContext { SizeScale = 1f, Speed = 5f };

        [Header("Skill Runtime")]
        [SerializeField] protected DamageContext damageContext = new DamageContext(1, 0.1f, 0f);
        [SerializeField] protected AttackResetTiming resetTiming = AttackResetTiming.OnSkillFinished;
        [SerializeField] protected float projectileDistanceStep;
        [SerializeField] protected float projectileAngleStep;
        [SerializeField] protected int spreadProjectileCount;
        [SerializeField] protected float spreadDamageScale = 1f;
        [SerializeField] protected int parallelProjectileCount;
        [SerializeField] protected float parallelDamageScale;
        [SerializeField] protected string explosiveAssetName;
        [SerializeField] protected float explosiveRadius;
        [SerializeField] protected float explosiveDamageScale;
        [SerializeField] protected float instantKillTargetBelowHealthPercent;

        [Header("Animation & Attack Timing")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected string attackTrigger = "Attack";
        [SerializeField] protected float attackWindupDelay = 0.12f;
        [SerializeField] protected float attackDuration = 0.35f;

        [Header("Rotate")]
        [SerializeField] protected Transform muzzle; // nguồn bắn đạn (null → transform.position)
        [SerializeField] protected Transform pivot;  // tâm xoay (null → transform.position)
        [SerializeField] protected AnimationCurve rotateCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] protected float rotateBaseDuration = 0.25f;
        [SerializeField] protected bool flipSprite = true;
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Header("Audio")]
        [SerializeField] protected AudioClip[] attackAudioClips;

        [Header("Outline theo Level")]
        [SerializeField] protected Color outlineColorDefault = Color.white;
        [SerializeField] protected Color outlineColorX2 = new Color(1f, 0.85f, 0f, 1f);
        [SerializeField] protected Color outlineColorX3 = new Color(1f, 0.2f, 0.1f, 1f);
        [SerializeField] protected string outlineColorProperty = "_Color";
        [SerializeField] protected Material outlineX2Material;
        [SerializeField] protected Material outlineX3Material;

        [Header("Upgrade")]
        [SerializeField] protected int startLevel = 1;

        /* =====================================================================================
         * Runtime state
         * ===================================================================================== */
        protected bool isInitialized;
        protected bool isAttacking;
        protected bool hasTarget;
        protected float cooldownTimer;

        // Mục tiêu hiện tại (entity + destination)
        protected int primaryTargetEntity;
        protected int secondaryTargetEntity;
        protected Vector3 primaryDestination;
        protected Vector3 secondaryDestination;

        // Level / Power
        protected int level;
        protected int powerLevel = 1; // 1 | 2 | 3 -> Power X1 / X2 / X3
        protected readonly List<WeaponUpgradeData> appliedUpgrades = new List<WeaponUpgradeData>();

        // Pause / TimeScale
        protected float timeScale = 1f;
        protected bool isPaused;

        // Cache
        protected Coroutine attackRoutine;
        protected MaterialPropertyBlock outlineBlock;
        protected Renderer outlineRenderer;
        protected AudioSource cachedAudioSource;

        public bool IsInitialized => isInitialized;
        public bool IsAttacking => isAttacking;
        public int Level => level;
        public int PowerLevel => powerLevel;
        public int PrimaryTargetEntity => primaryTargetEntity;
        public int SecondaryTargetEntity => secondaryTargetEntity;

        /// <summary>deltaTime của riêng weapon – tôn trọng Pause + TimeScale.</summary>
        protected float DeltaTime => isPaused || Mathf.Approximately(timeScale, 0f) ? 0f : Time.deltaTime * timeScale;

        // Events
        public event Action OnAttackStarted;
        public event Action OnAttackPerformed;
        public event Action OnAttackStopped;
        public event Action<ProjectilePhase> OnProjectilePhase;

        /* =====================================================================================
         * 1. KHỞI TẠO
         * ===================================================================================== */
        protected virtual void Awake()
        {
            if (owner == null) owner = GetComponentInParent<HeroController>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            outlineRenderer = spriteRenderer;
            if (animator == null) animator = GetComponent<Animator>();
            cachedAudioSource = GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            level = Mathf.Max(1, startLevel);
            powerLevel = 1;
            isInitialized = true;
            isAttacking = false;
            cooldownTimer = 0f;

            // ponytail: struct serialize trên scene cũ có thể mang Duration = 0 -> đạn tắt ngay frame đầu
            if (projectileContext.Duration <= 0f) projectileContext.Duration = 2f;

            CacheAudio();
            UpdateGroupData();
        }

        /// <summary>Cache âm thanh ngay khi khởi tạo để PlayOneShot không bị giật (hit IO).</summary>
        protected virtual void CacheAudio()
        {
            if (cachedAudioSource == null && attackAudioClips != null && attackAudioClips.Length > 0)
            {
                cachedAudioSource = gameObject.AddComponent<AudioSource>();
                cachedAudioSource.playOnAwake = false;
                cachedAudioSource.spatialBlend = 0f;
                cachedAudioSource.volume = 1f;
            }
        }

        /* =====================================================================================
         * 2. QUẢN LÝ NÂNG CẤP / LEVEL
         * ===================================================================================== */
        /// <summary>Danh sách upgrade khả dụng ở level hiện tại. Subclass override để lấy từ nguồn dữ liệu thật.</summary>
        public virtual List<WeaponUpgradeData> GetUpgradeDataAvailable()
        {
            return new List<WeaponUpgradeData>
            {
                new WeaponUpgradeData { projectileDamage = 0.1f, attackRange = 0.25f, cooldownReduction = 0.05f },
                new WeaponUpgradeData { spreadProjectile = 1 },
            };
        }

        /// <summary>Áp dụng một upgrade cụ thể, tăng level, cập nhật group/power/outline.</summary>
        public virtual bool UpgradeData(WeaponUpgradeData data)
        {
            if (data == null) return false;

            data.Increase();
            appliedUpgrades.Add(data);
            level++;
            UpdateGroupData();
            return true;
        }

        /// <summary>Tự chọn upgrade đầu tiên khả dụng và áp dụng (dùng khi auto-upgrade).</summary>
        public virtual bool IncreaseUpgradeData()
        {
            List<WeaponUpgradeData> available = GetUpgradeDataAvailable();
            if (available == null || available.Count == 0) return false;
            return UpgradeData(available[0]);
        }

        /// <summary>Đánh giá lại group/power khi level thay đổi. Điểm để hook hệ thống nhóm nâng cấp.</summary>
        public virtual void UpdateGroupData()
        {
            // Power X2 / X3 theo level (subclass override lại luật nếu khác)
            powerLevel = level >= 5 ? 3 : level >= 3 ? 2 : 1;
            ApplyOutlineVisual();
        }

        /// <summary>Damage thực tế = Attack base * (1 + tổng bonus upgrade) * Power.</summary>
        protected virtual int EffectiveAttack()
        {
            float bonus = 1f;
            for (int i = 0; i < appliedUpgrades.Count; i++)
            {
                bonus += appliedUpgrades[i].projectileDamage;
            }

            return Mathf.RoundToInt(attack * bonus * powerLevel);
        }

        protected virtual float EffectiveAttackRange()
        {
            float bonus = 0f;
            for (int i = 0; i < appliedUpgrades.Count; i++)
            {
                bonus += appliedUpgrades[i].attackRange;
            }

            return Mathf.Max(0.1f, attackRange + bonus);
        }

        protected virtual float EffectiveCooldown()
        {
            float reduce = 0f;
            for (int i = 0; i < appliedUpgrades.Count; i++)
            {
                reduce += appliedUpgrades[i].cooldownReduction;
            }

            return Mathf.Max(0.05f, attackCooldown * (1f - reduce));
        }

        /* =====================================================================================
         * 3. AUTO ATTACK – vòng lặp chính
         *    Cooldown → FindTarget → Rotate → Play Attack → [Animation Event] →
         *    ExecuteAttack → StopAttack → quay lại Cooldown
         * ===================================================================================== */
        protected virtual void Update()
        {
            if (!isInitialized || isPaused || isAttacking) return;

            cooldownTimer -= DeltaTime;
            if (cooldownTimer > 0f) return;

            if (FindTarget())
            {
                StartAttack();
            }
            else
            {
                // Không có mục tiêu -> thử lại sau một khoảng ngắn
                cooldownTimer = retryFindTargetDelay;
            }
        }

        protected virtual void StartAttack()
        {
            isAttacking = true;
            OnAttackStarted?.Invoke();

            attackRoutine = StartCoroutine(AttackSequenceRoutine());
        }

        protected virtual IEnumerator AttackSequenceRoutine()
        {
            // (1) Xoay về hướng mục tiêu
            yield return RotateToRoutine(GetDestination());

            // (2) Play attack animation + audio
            OnPlayAttack();

            // (3) Có Animator -> Animation Event trong Animator sẽ gọi ExecuteAttack / StopAttack
            if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            {
                animator.SetTrigger(attackTrigger);
                yield break;
            }

            // Không có Animator -> tự chủ động theo timer
            yield return WaitScaled(attackWindupDelay);

            ExecuteAttack();

            if (resetTiming == AttackResetTiming.OnAnimationFinished)
            {
                yield return WaitScaled(attackDuration);
                StopAttack();
            }
            else
            {
                // OnSkillFinished -> SkillFactory sẽ gọi StopAttack() khi skill hoàn tất.
                // Đợi ở đây để giữ trạng thái đang tấn công.
                while (isAttacking && !isPaused) yield return null;
            }
        }

        private IEnumerator WaitScaled(float duration)
        {
            float t = 0f;
            while (t < duration && !isPaused)
            {
                t += DeltaTime;
                yield return null;
            }
        }

        /* =====================================================================================
         * 4. FIND TARGET
         * ===================================================================================== */
        protected virtual bool FindTarget()
        {
            primaryTargetEntity = secondaryTargetEntity = 0;
            primaryDestination = secondaryDestination = Vector3.zero;
            hasTarget = false;

            MonsterTickRunner runner = MonsterTickRunner.Instance;
            if (runner == null) return false;

            Vector3 center = GetPivotPosition();
            Vector3 size = new Vector3(EffectiveAttackRange(), EffectiveAttackRange(), 0f);

            int count = runner.QueryAgentInRange(center, size, out AgentData[] results);
            if (count == 0) return false;

            // Primary = mục tiêu chính, Secondary = mục tiêu dự phòng (gần thứ 2 / xa thứ 2)
            float bestPrimarySqr = float.MaxValue;
            float bestSecondarySqr = float.MaxValue;
            Vector3 bestPrimaryPos = Vector3.zero;
            Vector3 bestSecondaryPos = Vector3.zero;
            float range = EffectiveAttackRange();
            float rangeSqr = range * range;

            // SC04: mục tiêu bị ép (force target) được ưu tiên trục tiếp nếu còn sống và trong tàm
            if (owner != null && owner.HasForcedTarget && runner.IsAlive(owner.ForcedTargetEntity)
                && runner.TryGetAgent(owner.ForcedTargetEntity, out AgentData forcedData))
            {
                Vector2 forcedPos = new Vector2(forcedData.position.x, forcedData.position.y);
                if ((forcedPos - (Vector2)center).sqrMagnitude <= rangeSqr)
                {
                    primaryTargetEntity = owner.ForcedTargetEntity;
                    primaryDestination = forcedPos;
                    hasTarget = true;
                    return true;
                }
            }

            for (int i = 0; i < count; i++)
            {
                AgentData data = results[i];

                // Bỏ qua mục tiêu đã chết (không còn trong registry)
                if (!runner.IsAlive(data.agent)) continue;

                Vector2 pos = new Vector2(data.position.x, data.position.y);
                float distSqr = (pos - (Vector2)center).sqrMagnitude;

                // Kiểm tra khoảng cách
                if (distSqr > rangeSqr) continue;

                switch (findTargetType)
                {
                    case FindTargetType.Nearest:
                        if (distSqr < bestPrimarySqr)
                        {
                            // đẩy primary cũ xuống secondary
                            secondaryTargetEntity = primaryTargetEntity;
                            bestSecondarySqr = bestPrimarySqr;
                            bestSecondaryPos = bestPrimaryPos;

                            primaryTargetEntity = data.agent;
                            bestPrimarySqr = distSqr;
                            bestPrimaryPos = pos;
                        }
                        else if (distSqr < bestSecondarySqr)
                        {
                            secondaryTargetEntity = data.agent;
                            bestSecondarySqr = distSqr;
                            bestSecondaryPos = pos;
                        }
                        break;

                    case FindTargetType.Farthest:
                        if (distSqr > bestPrimarySqr)
                        {
                            secondaryTargetEntity = primaryTargetEntity;
                            bestSecondarySqr = bestPrimarySqr;
                            bestSecondaryPos = bestPrimaryPos;

                            primaryTargetEntity = data.agent;
                            bestPrimarySqr = distSqr;
                            bestPrimaryPos = pos;
                        }
                        break;

                    default:
                        break;
                }
            }

            if (primaryTargetEntity == 0) return false;

            hasTarget = true;
            primaryDestination = bestPrimaryPos;
            secondaryDestination = bestSecondaryPos;
            return true;
        }

        /* =====================================================================================
         * 5. ROTATE
         * ===================================================================================== */
        protected virtual IEnumerator RotateToRoutine(Vector3 destination)
        {
            Vector3 muzzlePos = GetMuzzlePosition();
            Vector3 direction = destination - muzzlePos;
            if (direction.sqrMagnitude < 0.0001f) yield break;

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bool faceLeft = direction.x < 0f;
            bool applyFlip = flipSprite && faceLeft;

            // Flip sprite khi đổi hướng
            if (spriteRenderer != null) spriteRenderer.flipX = applyFlip;

            // Khi flip thì ảnh bị mirror -> góc hiển thị phải bù lại 180°
            float visualTarget = applyFlip ? 180f - targetAngle : targetAngle;
            float startAngle = transform.eulerAngles.z;
            float delta = Mathf.DeltaAngle(startAngle, visualTarget);

            // Tự điều chỉnh thời gian xoay theo chênh lệch góc
            float duration = rotateBaseDuration * Mathf.Clamp01(Mathf.Abs(delta) / 180f);

            float t = 0f;
            while (t < duration && !isPaused)
            {
                t += DeltaTime;
                float k = rotateCurve.Evaluate(Mathf.Clamp01(t / Mathf.Max(0.0001f, duration)));
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(startAngle, startAngle + delta, k));
                yield return null;
            }

            transform.rotation = Quaternion.Euler(0f, 0f, startAngle + delta);
        }

        /* =====================================================================================
         * 6. PLAY ATTACK
         * ===================================================================================== */
        protected virtual void OnPlayAttack()
        {
            PlayAudioAttackOneShot();
        }

        /* =====================================================================================
         * 7. EXECUTE ATTACK -> SkillFactory.BuildSkill
         * ===================================================================================== */
        /// <summary>Animation Event trong Animator gọi hàm này ở frame bắn đạn.</summary>
        public virtual void OnAttackAnimationEvent()
        {
            ExecuteAttack();
        }

        public virtual void ExecuteAttack()
        {
            if (!hasTarget) return;

            try
            {
                SkillContext skillContext = GetSkillData();
                SkillFactory.BuildSkill(skillContext);
            }
            catch (Exception e)
            {
                // SkillFactory chưa được khởi tạo (Runner null) -> không được kẹt trạng thái attacking
                Debug.LogError($"[Weapon] BuildSkill failed: {e}");
                StopAttack();
                return;
            }

            OnAttackPerformed?.Invoke();
            PlayAudioAttackOneShot();
        }

        /* =====================================================================================
         * 8. STOP ATTACK
         * ===================================================================================== */
        /// <summary>Kết thúc đợt tấn công, gọi callback/event, khởi động lại cooldown.</summary>
        public virtual void StopAttack()
        {
            if (!isAttacking) return;

            isAttacking = false;
            hasTarget = false;

            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            if (animator != null) animator.ResetTrigger(attackTrigger);

            OnStopAttack();
            OnAttackStopped?.Invoke();

            // Bắt đầu lại chu kỳ
            cooldownTimer = EffectiveCooldown();
        }

        protected virtual void OnStopAttack()
        {
        }

        /// <summary>Skill kết thúc -> SkillFactory gọi (khi ResetTiming = OnSkillFinished).</summary>
        public virtual void OnAttackSkillFinished()
        {
            if (resetTiming == AttackResetTiming.OnSkillFinished)
            {
                StopAttack();
            }
        }

        /// <summary>Animation Event ở cuối attack animation.</summary>
        public virtual void OnAttackAnimationEnd()
        {
            if (resetTiming == AttackResetTiming.OnAnimationFinished)
            {
                StopAttack();
            }
        }

        /* =====================================================================================
         * 9. PAUSE / RESUME / TIME SCALE / DELTA TIME
         * ===================================================================================== */
        public virtual void Pause()
        {
            isPaused = true;
        }

        public virtual void Resume()
        {
            isPaused = false;
        }

        public virtual void SetTimeScale(float scale)
        {
            timeScale = Mathf.Max(0f, scale);
        }

        public float TimeScale => timeScale;

        /* =====================================================================================
         * 10. OUTLINE theo level (MaterialPropertyBlock – tối ưu hiệu năng)
         *     Level 1: màu mặc định | Level 2: Outline X2 | Level 3 trở lên: Outline X3
         * ===================================================================================== */
        protected void ApplyOutlineVisual()
        {
            if (outlineRenderer == null) return;

            // Ưu tiên material outline nếu được gán
            bool useX3 = level >= 3 && outlineX3Material != null;
            bool useX2 = level == 2 && outlineX2Material != null;

            if (useX3 || useX2)
            {
                outlineRenderer.sharedMaterial = useX3 ? outlineX3Material : outlineX2Material;
                return;
            }

            // Ngược lại: chỉ set màu qua MaterialPropertyBlock (không sinh material instance)
            if (outlineBlock == null) outlineBlock = new MaterialPropertyBlock();

            outlineRenderer.GetPropertyBlock(outlineBlock);

            Color color = level >= 3 ? outlineColorX3 : level == 2 ? outlineColorX2 : outlineColorDefault;

            if (!string.IsNullOrEmpty(outlineColorProperty))
            {
                outlineBlock.SetColor(outlineColorProperty, color);
            }

            outlineRenderer.SetPropertyBlock(outlineBlock);
        }

        /* =====================================================================================
         * 11. ĐIỂM MỞ – virtual cho từng loại weapon override
         * ===================================================================================== */
        protected virtual Vector3 GetPivotPosition()
        {
            return pivot != null ? pivot.position : transform.position;
        }

        protected virtual Vector3 GetMuzzlePosition()
        {
            return muzzle != null ? muzzle.position : transform.position;
        }

        /// <summary>Đích bay của đạn (mặc định: vị trí primary target). Override để bắn secondary/split.</summary>
        protected virtual Vector3 GetDestination()
        {
            return primaryDestination;
        }

        /// <summary>Tạo <see cref="SkillContext"/> trước khi gọi <see cref="SkillFactory.BuildSkill"/>. Override để tùy biến damage/crit/projectile/explosion/range.</summary>
        protected virtual SkillContext GetSkillData()
        {
            bool useEquipment = projectile != null;

            FireContext fire = new FireContext
            {
                EquipmentPivot = GetPivotPosition(),
                EquipmentMuzzle = GetMuzzlePosition(),
                ProjectileDestination = GetDestination(),
                UseEquipment = useEquipment,
                Equipment = this,
            };

            return new SkillContext(
                primaryTargetEntity,
                useEquipment ? string.Empty : projectileAssetName,
                projectileContext,
                damageContext,
                fire,
                resetTiming,
                EffectiveAttack(),
                critDamage,
                critRate,
                projectileDistanceStep,
                projectileAngleStep,
                spreadProjectileCount,
                spreadDamageScale,
                parallelProjectileCount,
                parallelDamageScale,
                explosiveAssetName,
                explosiveRadius,
                explosiveDamageScale,
                instantKillTargetBelowHealthPercent
            );
        }

        protected virtual void PlayAudioAttackOneShot()
        {
            if (cachedAudioSource == null || attackAudioClips == null || attackAudioClips.Length == 0) return;

            AudioClip clip = attackAudioClips[UnityEngine.Random.Range(0, attackAudioClips.Length)];
            cachedAudioSource.PlayOneShot(clip);
        }

        /// <summary>Điểm mở: kích hoạt weapon (StartAttack từ hệ thống ngoài).</summary>
        public virtual void UseWeapon()
        {
            if (!isInitialized) Initialize();
        }

        /* =====================================================================================
         * Phase callback – đăng ký với Projectile.OnPhaseChanged bởi SkillFactory
         * ===================================================================================== */
        public void OnProjectilePhaseChange(ProjectilePhase phase)
        {
            OnProjectilePhase?.Invoke(phase);

            if (phase == ProjectilePhase.Complete)
            {
                OnAttackSkillFinished();
            }
        }

        protected virtual void OnDestroy()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
        }
    }
}
