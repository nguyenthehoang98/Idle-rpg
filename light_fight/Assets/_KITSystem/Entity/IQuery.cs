using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Entity
{
    public interface IQuery
    {
        void RandomTargetPosition(Vector2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);
        void FarthestTargetPosition(Vector2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);
        void NearestTargetPosition(Vector2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);

        List<int> GetEntities(Vector2 center, Func<int, bool> funcFilterEntity, float radius);
        List<int> GetEntities(Vector2 center, Func<int, bool> funcFilterEntity, Vector2 size);
    }

    public struct QueryResult
    {
        public bool IsPrimaryValid;
        public int PrimaryEntity;
        public Vector2 PrimaryPosition;
        public bool IsSecondaryValid;
        public int SecondaryEntity;
        public Vector2 SecondaryPosition;
    }
}