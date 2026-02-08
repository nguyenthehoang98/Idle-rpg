using System;
using Geometry;
using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.AbilitySystem
{
    public sealed class ShapeLogic: IDisposable
    {
        private readonly ShapeArg source;
        private Shape shape;
        private float elapsed;

        public Shape Shape => shape;
        public float2 PrevPos { get; private set; }

        public ShapeLogic(ShapeArg source)
        {
            this.source = source;
            shape = new Shape {type = source.type, radius = source.radius, size = source.size};
        }

        public void Startup(float2 pos)
        {
            PrevPos = pos;
        }

        public void PreExecute(float deltaTime)
        {
            if (source.customValue)
            {
                elapsed += deltaTime;

                float f = math.clamp(elapsed / source.duration, 0, 1);
                float value = source.curve.Evaluate(f);
                shape = new Shape
                {
                    type = source.type,
                    radius = math.lerp(source.radius, source.extraRadius, value),
                    size = math.lerp(source.size, source.extraSize, value)
                };
            }
        }

        public void Execute(float2 center, Shape target, float2 targetPos, out bool hit)
        {
            if (GeometryUtils.Overlaps(shape, PrevPos, center, target, targetPos))
            {
                hit = true;
            }
            else
            {
                hit = GeometryUtils.Sweep(shape, PrevPos, center, target, targetPos);
            }
        }

        public void AfterExecute(float2 center)
        {
            PrevPos = center;
        }

        public void Shutdown()
        {
        }

        public void Dispose()
        {
        }
    }
}