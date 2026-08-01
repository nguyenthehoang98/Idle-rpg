using UnityEngine;

namespace _TDS.GameplayScene.Spawn
{
    struct SpawnTimer
    {
        private int total;

        private readonly float startTime;
        private readonly float endTime;

        private int spawnedCount;
        private float elapsedTime;

        public SpawnTimer(float startTime, float endTime, int total)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.total = total;
            spawnedCount = 0;
            elapsedTime = 0;
        }

        public int Spawn(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (elapsedTime < startTime) return 0;

            float duration = endTime - startTime;

            if (duration <= 0 || total <= 0) return 0;

            float progress = Mathf.Clamp01((elapsedTime - startTime) / duration);

            int expectedCount = Mathf.FloorToInt(progress * total);

            int spawnCount = expectedCount - spawnedCount;

            spawnedCount = expectedCount;

            return spawnCount;
        }

        public bool IsFinished => spawnedCount >= total;
    }
}