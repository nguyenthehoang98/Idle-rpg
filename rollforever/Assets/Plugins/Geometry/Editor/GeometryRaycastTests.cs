using NUnit.Framework;
using Unity.Mathematics;

namespace Geometry
{
    public class GeometryRaycastTests
    {
        [Test]
        public void Raycast_Circle_HitFront()
        {
            Circle circle = new Circle(new float2(5, 0), 1f);
            Ray ray = new Ray(new float2(0, 0), new float2(1, 0)); // Bắn từ gốc tọa độ sang phải
        
            GeometryRaycast.Raycast(ray, 10f, circle, out RayHit2D hit);

            Assert.IsTrue(hit.hit);
            Assert.AreEqual(4f, hit.length, 0.001f); // Khoảng cách tới mép hình tròn (5 - 1 radius)
            Assert.AreEqual(new float2(-1, 0), hit.normal); // Normal phải hướng ngược lại hướng bắn
        }

        [Test]
        public void Raycast_AABB_MissWhenParallel()
        {
            AABB box = new AABB(new float2(5, 5), new float2(7, 7));
            Ray ray = new Ray(new float2(0, 0), new float2(1, 0)); // Ray bắn ngang nhưng box ở phía trên
        
            GeometryRaycast.Raycast(ray, 10f, box, out RayHit2D hit);

            Assert.IsFalse(hit.hit);
        }
    }
}