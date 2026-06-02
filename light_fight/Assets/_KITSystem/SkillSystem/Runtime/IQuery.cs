using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    public interface IQuery
    {
        void RandomTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);
        void FarthestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);
        void NearestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);

        List<int> GetEntities(float2 center, Func<int, bool> funcFilterEntity, float radius);
        List<int> GetEntities(float2 center, Func<int, bool> funcFilterEntity, float2 size);
    }

    public struct QueryResult
    {
        public bool IsPrimaryValid;
        public int PrimaryEntity;
        public float2 PrimaryPosition;
        public bool IsSecondaryValid;
        public int SecondaryEntity;
        public float2 SecondaryPosition;
    }
}