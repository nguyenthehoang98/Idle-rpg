using System;
using _FightCode.Battle.Model;
using _FightCode.Battle.View;
using _FightCode.Config;
using _KITSystem.ExcelConfig;
using _KITSystem.Grid;
using _KITSystem.Resource;
using UnityEngine;

namespace _FightCode.Battle.Logic
{
    public class Monster : IDisposable
    {
        /// <summary>
        /// Dùng cho việc getcomponentdata
        /// </summary>
        public readonly int entity;
        /// <summary>
        /// Dùng lấy stat của monster
        /// </summary>
        public readonly int configId;
        /// <summary>
        /// Tính vi tri monster
        /// </summary>
        public readonly int agent;

        private BattleShare share;
        private MonsterAnimation animation;

        private AgentData agentData;
        private Vector3 position;
        private bool isMoving = true;

        public Monster(BattleShare share, BattleSetting setting, int entity, int configId, int agent)
        {
            MonsterConfig monsterConfig = KitConfigManager.Get<MonsterConfig>();
            monsterConfig.Find(configId, out var monsterData);

            if (setting.enableVisualize) AsyncInstantiate(monsterData.Path);
        
            this.share = share;
            this.entity = entity;
            this.configId = configId;
            this.agent = agent;
        }

        private async void AsyncInstantiate(string path)
        {
            GameObject go = await KitLoaded.LoadAsync<GameObject>(path, true);
            animation = KitPool.Instantiate(go).GetComponent<MonsterAnimation>();
            animation.SetPosition(position);
            animation.SetActive(true);
            animation.Move();
        }

        public void Tick(float deltaTime)
        {
            bool f = share.agentGrid.TryGetAgent(agent, out agentData);
            if (f)
            {
                position = new Vector3(agentData.position.x, agentData.position.y);

                if (animation != null)
                {
                    if (isMoving && agentData.isStopped)
                    {
                        isMoving = false;
                        animation.Idle();
                    }
                    else if (!isMoving && !agentData.isStopped)
                    {
                        isMoving = true;
                        animation.Move();
                    }
                
                    animation.SetPosition(position);
                }
            }
        }

        public void Draw()
        {
            Color color = agentData.isStopped ? Color.red : Color.green;
            DrawCircle(position, agentData.radius, 6, color);
        }

        public void BeHit()
        {
            if (animation != null) animation.BeHit();
        }
    
        static void DrawCircle(Vector3 center, float radius, int segments, Color color)
        {
            Vector3 prev = center + Vector3.right * radius;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Debug.DrawLine(prev, next, color);
                prev = next;
            }
        }

        public void Dispose()
        {
            if (animation != null) animation.Dead();
        }
    }
}
