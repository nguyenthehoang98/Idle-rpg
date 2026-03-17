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
        private SpawnLogic spawnLogic;
        private LevelDesign levelDesign;
        private bool isRunning = false;
        
        private async void Start()
        {
            Entity player = ECSFactory.BuildPlayer();
            LevelSpawnSO levelSpawn = await LevelSpawnSO.LoadSpawn(1);
            levelDesign = Instantiate(levelSpawn.design);
            shareData = new ShareData(player, levelSpawn);
            spawnLogic = new SpawnLogic(shareData);
            
            // todo: close loading scene
            KitEntryScene.Instance.CloseLoadingScene();
#if DEVELOP_MODE
            gameObject.AddComponent<CpuFrame>();
#endif
        }
    }
}
