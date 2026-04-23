using Unity.Entities;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct ProjectileBoomerangTrajectory : IComponentData
    {
        public bool EntryPhase;
        private BlobAssetReference<CurveBlob> CastBlob;
        private readonly float CastSpeed;
        private BlobAssetReference<CurveBlob> ReturnBlob;
        private readonly float ReturnSpeed;
        private readonly float Duration;

        public ProjectileBoomerangTrajectory(BlobAssetReference<CurveBlob> castBlob, float castSpeed, 
            BlobAssetReference<CurveBlob> returnBlob, float returnSpeed, 
            float duration)
        {
            CastBlob = castBlob;
            CastSpeed = castSpeed;
            ReturnBlob = returnBlob;
            ReturnSpeed = returnSpeed;
            Duration = duration;
            EntryPhase = true;
        }
    }
}