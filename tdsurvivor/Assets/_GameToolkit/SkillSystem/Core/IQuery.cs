using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Core
{
    public interface IQuery
    {
        void FindTarget(FindTargetType type, int totalQuery, Vector2 center, Vector2 pivot, float radius, Func<int, float2, bool> funcFilterEntity, out QueryResult result);

        List<int> GetAllEntities(Vector2 center, Vector2 size, Func<int, bool> funcFilterEntity);
        List<int> GetAllEntities(Vector2 center, Vector2 size, Vector2 direction, Func<int, bool> funcFilterEntity);
    }
}