using Geometry.Primary;
using NUnit.Framework;
using Unity.Mathematics;

namespace Geometry
{
    public class GeometrySweepTests
    {
        [Test]
        public void SweepCircleCircle_HitTarget()
        {
            // Hình tròn di động di chuyển từ (0,0) tới (10,0)
            float2 prev = new float2(0, 0);
            float2 curr = new float2(10, 0);
            float radiusA = 1f;

            // Hình tròn tĩnh tại (5, 0)
            Circle target = new Circle(new float2(5, 0), 1f);

            bool hit = GeometryUtils.Sweep(
                ShapeInstance.Create(ShapeType.Circle, radiusA), prev, curr,
                ShapeInstance.Create(ShapeType.Circle, target.radius), target.center
            );
            
            Assert.IsTrue(hit);
        }

        [Test]
        public void SweepCircleAABB_HitCorner()
        {
            /*// Test trường hợp bo tròn góc khi Circle quét qua góc của AABB
            AABB box = new AABB(new float2(5, 5), new float2(7, 7));
            float2 prev = new float2(2, 2);
            float2 curr = new float2(8, 8); // Di chuyển chéo qua góc dưới bên trái của box
            float radius = 1f;
            
            bool hit = GeometryUtils.Sweep(
                ShapeInstance.Create(ShapeType.Circle, radius), prev, curr,
                ShapeInstance.Create(ShapeType.Circle, target.radius), target.center
            );
            
            Assert.IsTrue(hit);

            GeometrySweep.SweepCircleAABB(prev, curr, radius, box, out RayHit2D hit);

            Assert.IsTrue(hit.hit);
            Assert.IsNotNull(hit.normal);*/
        }
    }
}