namespace Geometry
{
    using NUnit.Framework;
    using Unity.Mathematics;
    using Geometry;

    public class GeometryAABBTests
    {
        [Test]
        public void Overlaps_AABB_ReturnTrueWhenTouching()
        {
            AABB box1 = new AABB(new float2(0, 0), new float2(2, 2));
            AABB box2 = new AABB(new float2(1, 1), new float2(3, 3));
        
            Assert.IsTrue(GeometryAABB.Overlaps(box1, box2));
        }

        [Test]
        public void Contains_Point_ReturnCorrectResult()
        {
            AABB box = new AABB(new float2(0, 0), new float2(10, 10));
            float2 inside = new float2(5, 5);
            float2 outside = new float2(11, 5);

            Assert.IsTrue(GeometryAABB.Contains(box, inside));
            Assert.IsFalse(GeometryAABB.Contains(box, outside));
        }
    }
}