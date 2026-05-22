using System;
using System.Collections.Generic;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    [Serializable]
    public class Weapon
    {
        private IQuery query;
        private BattleSetting setting;
        private float scanRadius;
        private float forwardOffset = 1.5f;
        private float scaleTime = 1f;
        private float2 position;
        private float2 direction;
        private float2 defaultDirection;
        private float elapsedTime;
        private bool isPlaying;

        public Weapon(BattleSetting setting, IQuery query, float2 direction)
        {
            this.query = query;
            this.setting = setting;
            this.direction = defaultDirection = direction;
            this.position = position + direction * (forwardOffset * scaleTime);
        }

        public void Draw(float scale, Color color)
        {
            scaleTime = scale;
            DrawTriangle(color);
            DrawSquare(color);
        }

        public void Active() => isPlaying = true;
        
        public void Inactive() => isPlaying = false;

        public void Tick(float dt)
        {
            if (isPlaying)
            {
                elapsedTime += dt;
                if (elapsedTime >= setting.weaponCooldown)
                {
                    elapsedTime = 0;

                    bool found = query.FindNearestTargetPosition(setting.center, setting.weaponAttackRange, out float2 targetPosition);
                    if (found) RotateTo(targetPosition);
                }
            }
        }
        
        void DrawTriangle(Color color)
        {
            float height = 0.4f;

            float2 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            float2 center = position + offset;
            float2 right = new float2(-direction.y, direction.x);
            
            float halfBase = height / 3;
            float2 vector = direction * height;
            float2 v3 = center + vector * 0.75f;
            float2 baseCenter = center;

            float2 v1 = baseCenter - right * halfBase;
            float2 v2 = baseCenter + right * halfBase;

            Debug.DrawLine(new Vector3(v1.x, v1.y), new Vector3(v2.x, v2.y), color);
            Debug.DrawLine(new Vector3(v2.x, v2.y), new Vector3(v3.x, v3.y), color);
            Debug.DrawLine(new Vector3(v3.x, v3.y), new Vector3(v1.x, v1.y), color);
        }
        
        void DrawSquare(Color color)
        {
            float size = 0.1f;

            float2 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            float2 center = position + offset;
            float2 right = new float2(-direction.y, direction.x);
            float half = size * 0.5f;

            // 4 góc hình vuông
            float2 v1 = center - right * half - direction * half;
            float2 v2 = center + right * half - direction * half;
            float2 v3 = center + right * half + direction * half;
            float2 v4 = center - right * half + direction * half;

            // draw
            Debug.DrawLine(new Vector3(v1.x, v1.y), new Vector3(v2.x, v2.y), color);
            Debug.DrawLine(new Vector3(v2.x, v2.y), new Vector3(v3.x, v3.y), color);
            Debug.DrawLine(new Vector3(v3.x, v3.y), new Vector3(v4.x, v4.y), color);
            Debug.DrawLine(new Vector3(v1.x, v1.y), new Vector3(v4.x, v4.y), color);
        }

        public void RotateTo(float2 worldPos)
        {
            direction = MathUtils.NormalizeSafe(worldPos - position);
            float2 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            float2 center = position + offset;
            Debug.DrawRay(
                new Vector3(center.x, center.y),
                new Vector3(direction.x, direction.y) * 10,
                Color.magenta,
                setting.weaponCooldown * 0.6f
            );
        }
    }
}