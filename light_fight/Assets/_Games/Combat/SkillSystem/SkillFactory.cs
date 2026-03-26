using System.Collections.Generic;
using _Games.Combat.SkillSystem.Config;
using _Games.Combat.SkillSystem.Model;
using _KIT.Resource;
using Cysharp.Threading.Tasks;

namespace _Games.Combat.SkillSystem
{
    public static class SkillFactory
    {
        static Dictionary<int, Skill> container = new Dictionary<int, Skill>();
        static HashSet<string> cache = new HashSet<string>();

        public static bool FindSkill(int skillID, out Skill skill)
        {
            return container.TryGetValue(skillID, out skill);
        }
        
        public static async UniTask<Skill> CreateSkill(SkillData data)
        {
            if (container.TryGetValue(data.SkillId, out Skill skill))
                return skill;

            SkillMainModule main = new SkillMainModule
            {
                lifeTime = data.LifeTime,
                type = data.Type,
                needTargetToCast = data.NeedTargetToCast,
                maxTargetRange = data.MaxFindTargetRange,
                maxHitCount = data.MaxHitCount,
                collisionResetInterval = data.CollisionResetInterval
            };

            ProjectileSO projectile = await KitLoaded.LoadAsync<ProjectileSO>(data.ProjectileId, true);
            cache.Add(data.ProjectileId);
            BaseColliderSO collider = await KitLoaded.LoadAsync<BaseColliderSO>(data.ColliderId, true);
            cache.Add(data.ColliderId);
            BaseTrajectorySO trajectory = await KitLoaded.LoadAsync<BaseTrajectorySO>(data.TrajectoryId, true);
            cache.Add(data.TrajectoryId);
            BaseModifierSO[] modifiers = new BaseModifierSO[data.ModifiersId.Length];
            for (int i = 0; i < data.ModifiersId.Length; i++)
            {
                modifiers[i] = await KitLoaded.LoadAsync<BaseModifierSO>(data.ModifiersId[i], true);
                cache.Add(data.ModifiersId[i]);
            }
            BaseBehaviorSO[] behaviors = new BaseBehaviorSO[data.BehaviorsId.Length];
            for (int i = 0; i < data.BehaviorsId.Length; i++)
            {
                behaviors[i] = await KitLoaded.LoadAsync<BaseBehaviorSO>(data.BehaviorsId[i], true);
                cache.Add(data.BehaviorsId[i]);
            }
            
            skill = new Skill(data.SkillId, main, projectile, collider, trajectory, modifiers, behaviors);
            container.Add(data.SkillId, skill);
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