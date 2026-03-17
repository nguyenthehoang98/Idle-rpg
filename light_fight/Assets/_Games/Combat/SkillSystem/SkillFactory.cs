using System.Collections.Generic;
using _Games.Combat.SkillSystem.Config;
using _Games.Combat.SkillSystem.Model;
using _KIT.Resource;
using Cysharp.Threading.Tasks;

namespace _Games.Combat.SkillSystem
{
    public static class SkillFactory
    {
        static Dictionary<string, Skill> container = new Dictionary<string, Skill>();
        static HashSet<string> cache = new HashSet<string>();
        
        public static async UniTask<Skill> CreateSkill(SkillData data)
        {
            if (container.TryGetValue(data.skillId, out Skill skill))
                return skill;

            SkillMainModule main = new SkillMainModule
            {
                lifeTime = data.lifeTime,
                castTime = data.castTime,
                type = data.type,
                needTargetToCast = data.needTargetToCast,
                maxTargetRange = data.maxTargetRange,
                maxHitCount = data.maxHitCount,
                collisionResetInterval = data.collisionResetInterval
            };

            ProjectileSO projectile = await KitLoaded.LoadAsync<ProjectileSO>(data.projectileId, true);
            cache.Add(data.projectileId);
            BaseColliderSO collider = await KitLoaded.LoadAsync<BaseColliderSO>(data.colliderId, true);
            cache.Add(data.colliderId);
            BaseTrajectorySO trajectory = await KitLoaded.LoadAsync<BaseTrajectorySO>(data.trajectoryId, true);
            cache.Add(data.trajectoryId);
            BaseModifierSO[] modifiers = new BaseModifierSO[data.modifiersId.Length];
            for (int i = 0; i < data.modifiersId.Length; i++)
            {
                modifiers[i] = await KitLoaded.LoadAsync<BaseModifierSO>(data.modifiersId[i], true);
                cache.Add(data.modifiersId[i]);
            }
            BaseBehaviorSO[] behaviors = new BaseBehaviorSO[data.behaviorsId.Length];
            for (int i = 0; i < data.behaviorsId.Length; i++)
            {
                behaviors[i] = await KitLoaded.LoadAsync<BaseBehaviorSO>(data.behaviorsId[i], true);
                cache.Add(data.behaviorsId[i]);
            }
            
            skill = new Skill(main, projectile, collider, trajectory, modifiers, behaviors);
            container.Add(data.skillId, skill);
            return skill;
        }

        public static void UnloadAll()
        {
            foreach (var path in cache)
            {
                KitLoaded.UnCache(path);
            }

            container.Clear();
            cache.Clear();
        }
    }
}