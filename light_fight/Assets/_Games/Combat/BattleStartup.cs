using _Games.Combat.EntityComponentSystem;
using _Games.Combat.Level;
using _Games.Combat.Model;
using _KIT.Checker;
using _KIT.Utils;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat
{
    public class BattleStartup : MonoBehaviour
    {
        private ShareData shareData;
        private LevelDesign levelDesign;
        private bool isRunning = false;
        
        private async void Start()
        {
            Entity player = ECSFactory.BuildPlayer();
            shareData = new ShareData(player);
       
            LevelSpawnSO levelSpawn = await LevelSpawnSO.LoadSpawn(1);
            levelDesign = Instantiate(levelSpawn.design);
            // todo: close loading scene
            KitEntryScene.Instance.CloseLoadingScene();
#if DEVELOP_MODE
            gameObject.AddComponent<CpuFrame>();
#endif
        }
    }
}
