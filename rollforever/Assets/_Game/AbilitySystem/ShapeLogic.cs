using System;
using Geometry;
using Geometry.Primary;
using Unity.Mathematics;

namespace _Game.AbilitySystem
{
    public sealed class ShapeLogic: IDisposable
    {
        private readonly Shape source;
        public float2 prevPos { get; private set; }

        public ShapeLogic(Shape source)
        {
            this.source = source;
        }

        public void Startup(float2 pos)
        {
            prevPos = pos;
        }

        public void PreExecute()
        {
            
        }

        public void Execute(float2 center, Shape target, float2 targetPos, float deltaTime, out bool hit)
        {
            if (GeometryUtils.Overlaps(source, prevPos, center, target, targetPos))
            {
                hit = true;
            }
            else
            {
                hit = GeometryUtils.Sweep(source, prevPos, center, target, targetPos);
            }
        }

        public void AfterExecute(float2 center)
        {
            prevPos = center;
        }

        public void Shutdown()
        {
        }

        public void Dispose()
        {
        }
    }
}