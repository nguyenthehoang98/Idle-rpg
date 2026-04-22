using System;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.Equipment;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Combat.Model;
using _Games.Config;
using _Games.Misc.Model;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using _KIT.Resource;
using _KIT.Utils;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Combat
{
    public class DevelopmentBattleStartup : KitEntryScene
    {
        public TMP_InputField weaponInputField;
        public Button buttonPickWeapon;
        
        private ShareData shareData;
        private SpawnLogic spawnLogic;
        private LevelDesign levelDesign;
        private EquipmentManager equipmentManager;

        protected override async void OnStart()
        {
            buttonPickWeapon.onClick.AddListener(PickWeapon);
            
            // todo: load default config
            await KitConfigManager.Load(new[]
            {
                "MonsterConfig",
                "SkillConfig",
                "LevelConfig",
                "WeaponConfig",
            });

            await KitLoaded.LoadAsync<RaritySO>(GlobalsPath.RARITY_SO, true);            
            
            // todo: build world
            if(World.DefaultGameObjectInjectionWorld != null) World.DefaultGameObjectInjectionWorld.Dispose();
            DefaultWorldInitialization.Initialize("GameWorld", false);
            
            // todo: init level spawn
            int levelId = 1;
            GameObject go = await KitLoaded.LoadAsync<GameObject>(GlobalsPath.GetLevelDesignPath(levelId));
            levelDesign = Instantiate(go).GetComponent<LevelDesign>();
            levelDesign.Initialize(1);
            Entity player = ECSFactory.BuildPlayer(levelDesign);
            
            // todo: register object
            //shareData = new ShareData(dictionary, levelDesign);
            //spawnLogic = new SpawnLogic(shareData, player);
            equipmentManager = new EquipmentManager(levelDesign);
            levelDesign.OnTriggerWeapon += equipmentManager.Trigger;
            
            EventBus.Instance.Publish(new WaveContinueEvent());
        }
        
        void PickWeapon()
        {
            int slot = 0;
            Rarity rarity = Rarity.Common;
            if (int.TryParse(weaponInputField.text, out int value))
            {
                WeaponConfig weaponConfig = KitConfigManager.Get<WeaponConfig>();
                if (weaponConfig.Find(value, out var weaponData))
                {
                    SlotItem slotItem = levelDesign.Slots[slot];
                    slotItem.Equip(weaponData, rarity);
                    equipmentManager.Equip(slot, weaponData, RarityMethod.ParseLevel(rarity));
                }
                else
                {
                    Debug.LogError($"Not found weapon_id '{value}'");
                    weaponInputField.text = String.Empty;
                }
            }
        }
    }
}