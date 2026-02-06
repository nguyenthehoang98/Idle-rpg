using NUnit.Framework;
using Unity.Mathematics;

namespace Geometry
{
    public class GeometryOBBTests
    {
        [Test]
        public void Overlaps_OBB_With_Circle_WhenRotated()
        {
            // OBB tại (0,0) xoay 45 độ
            OBB obb = new OBB(new float2(0, 0), new float2(2, 0.5f), math.PI / 4);
        
            // Circle đặt tại vị trí mà nếu không xoay thì không chạm, nhưng xoay thì chạm
            Circle c = new Circle(new float2(1.5f, 1.5f), 0.5f);

            Assert.IsTrue(GeometryOBB.Overlaps(obb, c));
        }
    }
}