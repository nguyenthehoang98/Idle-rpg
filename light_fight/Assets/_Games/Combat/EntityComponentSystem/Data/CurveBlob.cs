using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Data
{
    public struct CurveBlob
    {
        public BlobArray<float> Samples;
        
        public static float Evaluate(ref CurveBlob blob, float t)
        {
            ref BlobArray<float> samples = ref blob.Samples;
            float index = t * (samples.Length - 1);
            int i0 = (int)math.floor(index);
            int i1 = math.min(i0 + 1, samples.Length - 1);
            float frac = index - i0;
            return math.lerp(samples[i0], samples[i1], frac);
        }
        
        public static BlobAssetReference<CurveBlob> CreateCurveBlob(AnimationCurve curve)
        {
            int sampleCount = math.clamp(curve.keys.Length * 8, 16, 128);

            using var builder = new BlobBuilder(Allocator.Temp);
            ref CurveBlob root = ref builder.ConstructRoot<CurveBlob>();

            var samples = builder.Allocate(ref root.Samples, sampleCount);

            float start = curve.keys[0].time;
            float end = curve.keys[curve.length - 1].time;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)(sampleCount - 1);
                float time = Mathf.Lerp(start, end, t);

                samples[i] = curve.Evaluate(time);
            }

            return builder.CreateBlobAssetReference<CurveBlob>(Allocator.Persistent);
        }
    }
}