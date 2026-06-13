using UnityEngine;

namespace _Games.GamePlay.SpawnerSystem
{
    [CreateAssetMenu]
    public class LevelAsset : ScriptableObject
    {
        public GameObject backgroundPrefab;
        public WaveData[] WavesData;
    }
}