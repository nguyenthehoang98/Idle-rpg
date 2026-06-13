using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.SkillSystem.Core
{
    public interface IQuery
    {
        void FindTarget(FindTargetType type, Vector2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);

        List<int> GetAllEntities(Vector2 center, Vector2 size, Func<int, bool> funcFilterEntity);
    }
    
    public struct QueryResult
    {
        public QueryEntityData Primary;
        public QueryEntityData Secondary;
    }

    public readonly struct QueryEntityData
    {
        public readonly bool IsValid;
        public readonly int Entity;
        public readonly Vector2 Position;

        public QueryEntityData(int entity, Vector2 position)
        {
            IsValid = true;
            Entity = entity;
            Position = position;
        }
    }
}