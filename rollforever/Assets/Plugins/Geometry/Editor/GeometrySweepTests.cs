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

            GeometrySweep.SweepCircleCircle(prev, curr, radiusA, target, out RayHit2D hit);

            Assert.IsTrue(hit.hit);
            // Va chạm xảy ra khi 2 tâm cách nhau = r1 + r2 = 2.0
            // Vậy tâm của Circle A sẽ ở vị trí (3, 0). Quãng đường di chuyển là 3.0
            Assert.AreEqual(3.0f, hit.length, 0.001f);
        }

        [Test]
        public void SweepCircleAABB_HitCorner()
        {
            // Test trường hợp bo tròn góc khi Circle quét qua góc của AABB
            AABB box = new AABB(new float2(5, 5), new float2(7, 7));
            float2 prev = new float2(2, 2);
            float2 curr = new float2(8, 8); // Di chuyển chéo qua góc dưới bên trái của box
            float radius = 1f;

            GeometrySweep.SweepCircleAABB(prev, curr, radius, box, out RayHit2D hit);

            Assert.IsTrue(hit.hit);
            Assert.IsNotNull(hit.normal);
        }
    }
}