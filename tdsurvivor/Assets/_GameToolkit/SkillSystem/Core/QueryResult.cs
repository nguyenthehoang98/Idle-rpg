using System.Collections.Generic;
using UnityEngine;

namespace _GameToolkit.SkillSystem.Core
{
    public struct QueryResult
    {
        public List<QueryEntityData> Results;
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