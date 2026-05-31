using System;
using _FightCode.Battle.Model;
using _FightCode.Battle.View;
using _FightCode.Config;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace _FightCode.Battle.Logic
{
    [Serializable]
    public class Weapon
    {
        private IQuery query;
        private BattleShare share;
        private IWeaponView view;
        private BattleSetting setting;
        private float scanRadius;
        private float forwardOffset = 1.5f;
        private float scaleTime = 1f;
        private Vector3 position;
        private Vector3 direction;
        private Vector3 defaultDirection;
        private Phase phase = Phase.Cooldown;
        private float elapsedTime;
        private bool needUpdatePosition;
        private bool isPlaying;

        public Weapon(int order, ISlotView slotView, BattleShare share, BattleSetting setting, IQuery query, Vector3 direction)
        {
            this.share = share;
            this.query = query;
            this.setting = setting;
            this.direction = defaultDirection = direction;
            this.position = position + direction * (forwardOffset * scaleTime);
            this.view = setting.weapon.Instantiate(order, setting, 0, 90, slotView);
        }

        public void Equip(int weaponId, int weaponLevel)
        {
            view.Equip(weaponId, weaponLevel);
        }
        
        public void Play()
        {
            view.Play(share.timeScale);
            Equip(1001, 1);
        }
        
        public void Draw(float scale, Color color)
        {
            scaleTime = scale;
            DrawTriangle(color);
            DrawSquare(color);
        }

        public void Activate(float delayActivate)
        {
            needUpdatePosition = true;
            
            elapsedTime = view.Activate(delayActivate, share.timeScale);
            
            isPlaying = true;
        }
        
        public void Deactivate(float delayActivate)
        {
            view.Deactivate(delayActivate, share.timeScale);
            
            isPlaying = false;
        }

        public void Tick(float deltaTime)
        {
            if (isPlaying)
            {
                elapsedTime -= deltaTime;

                if (elapsedTime > 0) return;

                switch (phase)
                {
                    case Phase.Cooldown:
                        elapsedTime = int.MaxValue;

                        Vector3 muzzle = view.MuzzlePosition;
                        float2 center = new float2(muzzle.x, muzzle.y);
                        float radius = setting.weaponAttackRange;
                        
                        float2 targetPosition = Unity.Mathematics.float2.zero;
                        bool found = false;
                        
                        TargetType targetType = setting.skillFrameConfig.defaultSkillConfig.targetType;
                        switch (targetType)
                        {
                            case TargetType.Nearest:
                                found = query.FindNearestTargetPosition(center, radius, out targetPosition);
                                break;
                            case TargetType.Farthest:
                                found = query.FindFarthestTargetPosition(center, radius, out targetPosition);
                                break;
                            case TargetType.Random:
                                found = query.FindRandomTargetPosition(center, radius, out targetPosition);
                                break;
                            default:
                                Debug.LogError("Not defined target type " + targetType);
                                break;
                        }

                        if (found)
                        {
                            elapsedTime = RotateTo(new Vector3(targetPosition.x, targetPosition.y), deltaTime);
                            phase = Phase.Rotate;
                            SkillFactory.Build(new float2(position.x, position.y), targetPosition, setting.skillFrameConfig);
                        }

                        break;
                    case Phase.Rotate:
                        elapsedTime = setting.weaponCooldown;
                        phase = Phase.Cooldown;
                        break;
                }
            }
        }

        private void DrawTriangle(Color color)
        {
            float height = 0.4f;

            Vector3 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            Vector3 center = position + offset;
            Vector3 right = new Vector3(-direction.y, direction.x);
            
            float halfBase = height / 3;
            Vector3 vector = direction * height;
            Vector3 v3 = center + vector * 0.75f;
            Vector3 baseCenter = center;

            Vector3 v1 = baseCenter - right * halfBase;
            Vector3 v2 = baseCenter + right * halfBase;

            Debug.DrawLine(new Vector3(v1.x, v1.y), new Vector3(v2.x, v2.y), color);
            Debug.DrawLine(new Vector3(v2.x, v2.y), new Vector3(v3.x, v3.y), color);
            Debug.DrawLine(new Vector3(v3.x, v3.y), new Vector3(v1.x, v1.y), color);
        }
        
        private void DrawSquare(Color color)
        {
            float size = 0.1f;

            Vector3 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            Vector3 center = position + offset;
            Vector3 right = new Vector3(-direction.y, direction.x);
            float half = size * 0.5f;

            // 4 góc hình vuông
            Vector3 v1 = center - right * half - direction * half;
            Vector3 v2 = center + right * half - direction * half;
            Vector3 v3 = center + right * half + direction * half;
            Vector3 v4 = center - right * half + direction * half;

            // draw
            Debug.DrawLine(new Vector3(v1.x, v1.y), new Vector3(v2.x, v2.y), color);
            Debug.DrawLine(new Vector3(v2.x, v2.y), new Vector3(v3.x, v3.y), color);
            Debug.DrawLine(new Vector3(v3.x, v3.y), new Vector3(v4.x, v4.y), color);
            Debug.DrawLine(new Vector3(v1.x, v1.y), new Vector3(v4.x, v4.y), color);
        }

        public float RotateTo(Vector3 worldPos, float deltaTime)
        {
            direction = MathUtils.NormalizeSafeVec3(worldPos - position);
            
#if UNITY_EDITOR
            if(!setting.enableVisualize)
            {
                Vector3 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
                Vector3 center = position + offset;
                Debug.DrawRay(
                    new Vector3(center.x, center.y),
                    new Vector3(direction.x, direction.y) * 10,
                    Color.magenta, deltaTime
                );
            }
#endif
            view.Rotate(worldPos, setting.weaponRotateDuration, share.timeScale, needUpdatePosition);

            needUpdatePosition = false;
            
            return setting.weaponCooldown / share.timeScale;
        }

        enum Phase
        {
            Cooldown,
            Rotate
        }
    }
}