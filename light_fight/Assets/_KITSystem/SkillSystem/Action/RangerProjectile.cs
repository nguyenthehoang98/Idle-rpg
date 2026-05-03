using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Action
{
    public partial class CastProjectileAction
    {
        public class RangerProjectile : BaseProjectile
        {
            [TitleGroup("Ranger"), Required]
            public GameObject prefab;
            public override ProjectileType Type => ProjectileType.Ranger;
        }
    }
}