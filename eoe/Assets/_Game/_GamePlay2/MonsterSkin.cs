using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game._GamePlay2
{
    public class MonsterSkin : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        
        public async UniTask UpdateSkin(string skinName)
        {
            Sprite sprite = await AssetBundleManager.GetAssetCached<Sprite>(skinName);
            
            body.sprite = sprite;
        }
    }
}
