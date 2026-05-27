
using System;
using _Games.Battle.Model;
using Unity.Mathematics;
using UnityEngine;

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

    public Monster(BattleShare share, int entity, int configId, int agent)
    {
        this.share = share;
        this.entity = entity;
        this.configId = configId;
        this.agent = agent;
    }

    public void Tick(float deltaTime)
    {
        
    }

    public void Draw()
    {
        bool f = share.agentGrid.TryGetAgent(agent, out var agentData);
        if (f)
        {
            float2 position = agentData.position;
            Color color = agentData.isStopped ? Color.red : Color.green;
            DrawCircle(new Vector3(position.x, position.y), agentData.radius, 6, color);
        }
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
        
    }
}
