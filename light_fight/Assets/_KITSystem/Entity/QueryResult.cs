using UnityEngine;

namespace _KITSystem.Entity
{
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