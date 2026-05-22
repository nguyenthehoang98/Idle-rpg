using System;
using System.Collections.Generic;
using _KITSystem.SkillSystem.Runtime;
using _KITSystem.Utils;
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
        private Vector3 position;
        private Vector3 direction;
        private Vector3 defaultDirection;
        private float elapsedTime;
        private bool isPlaying;

        public Weapon(BattleSetting setting, IQuery query, Vector3 direction)
        {
            this.query = query;
            this.direction = defaultDirection = direction;
            this.position = position + direction.normalized * (forwardOffset * scaleTime);
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
                    List<int> units = query.GetUnits(setting.center, setting.weaponAttackRange);
                }
            }
        }
        
        void DrawTriangle(Color color)
        {
            float height = 0.4f;

            Vector3 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            Vector3 center = position + offset;
            Vector3 right = new Vector3(-direction.y, direction.x, 0f);
            
            float halfBase = height / 3;
            Vector3 vector = direction * height;
            Vector3 v3 = center + vector * 0.75f;
            Vector3 baseCenter = center;

            Vector3 v1 = baseCenter - right * halfBase;
            Vector3 v2 = baseCenter + right * halfBase;

            Debug.DrawLine(v1, v2, color);
            Debug.DrawLine(v2, v3, color);
            Debug.DrawLine(v3, v1, color);
        }
        
        void DrawSquare(Color color)
        {
            float size = 0.1f;

            Vector3 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            Vector3 center = position + offset;
            Vector3 right = new Vector3(-direction.y, direction.x, 0f);
            float half = size * 0.5f;

            // 4 góc hình vuông
            Vector3 v1 = center - right * half - direction * half;
            Vector3 v2 = center + right * half - direction * half;
            Vector3 v3 = center + right * half + direction * half;
            Vector3 v4 = center - right * half + direction * half;

            // draw
            Debug.DrawLine(v1, v2, color);
            Debug.DrawLine(v2, v3, color);
            Debug.DrawLine(v3, v4, color);
            Debug.DrawLine(v4, v1, color);
        }

        public void RotateTo(Vector3 worldPos)
        {
            direction = (worldPos - position).normalized;
            Vector3 offset = defaultDirection * (forwardOffset * (scaleTime - 1));
            Vector3 center = position + offset;
            Debug.DrawRay(center, direction * 10, Color.magenta, setting.weaponCooldown * 0.6f);
        }
    }
}