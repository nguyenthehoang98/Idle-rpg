using _TDS.GameConfig;
using UnityEngine;

namespace _TDS.Enemy
{
    public class Monster : MonoBehaviour
    {
        public void Initialize(Vector3 position, EnemyConfigData configData, MonsterRuntimeData runtimeData)
        {
            transform.position = position;
            transform.localScale = Vector3.one * runtimeData.SizeScale;
            gameObject.SetActive(true);
        }
    }
}