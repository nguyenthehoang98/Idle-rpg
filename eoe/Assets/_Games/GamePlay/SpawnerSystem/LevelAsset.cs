using UnityEngine;

namespace _Games.GamePlay.SpawnerSystem
{
    [CreateAssetMenu]
    public class LevelAsset : ScriptableObject
    {
        public WaveData[] WavesData;
    }
}