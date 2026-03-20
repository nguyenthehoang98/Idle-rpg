using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct CurveData : IComponentData
    {
        public BlobAssetReference<CurveBlob> Blob;

        public CurveData(AnimationCurve curve)
        {
            Blob = CurveBlob.CreateCurveBlob(curve);
        }

        public float Evaluate(float time)
        {
            return CurveBlob.Evaluate(ref Blob.Value, time);
        }
    }
}