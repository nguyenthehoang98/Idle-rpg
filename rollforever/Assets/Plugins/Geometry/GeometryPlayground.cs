using Geometry;
using UnityEngine;
using Ray = Geometry.Ray;

public class GeometryPlayground : MonoBehaviour
{
    [Header("Ray")]
    public Vector2 rayOrigin = new Vector2(-6, 0);
    public Vector2 rayDir = new Vector2(1, 0);

    [Header("Circle")]
    public Vector2 circleCenter = new Vector2(0, 1);
    public float circleRadius = 1.5f;

    [Header("AABB")]
    public Vector2 boxCenter = new Vector2(3, 0);
    public Vector2 boxSize = new Vector2(3, 2);

    [Header("Segment")]
    public Vector2 segA = new Vector2(-2, -2);
    public Vector2 segB = new Vector2(2, 2);

    public bool drawClosestPoint = true;

    void OnDrawGizmos()
    {
        // --- build primitives ---
        Ray ray = new Ray(
            new Vec2(rayOrigin.x, rayOrigin.y),
            new Vec2(rayDir.x, rayDir.y)
        );

        Circle circle = new Circle(new Vec2(circleCenter.x, circleCenter.y), circleRadius);
        AABB box = GeometryAABB.Build(new Vec2(boxCenter.x, boxCenter.y),
                                      new Vec2(boxSize.x, boxSize.y));

        Segment seg = new Segment(new Vec2(segA.x, segA.y),
                                  new Vec2(segB.x, segB.y));

        // --- draw ---
        GeometryRaycast.DrawGizmos(ray, Color.white, Time.deltaTime);
        GeometryCircle.DrawGizmos(circle, Color.cyan, Time.deltaTime);
        GeometryAABB.DrawGizmos(box, Color.yellow, Time.deltaTime);
        GeometrySegment.DrawGizmos(seg, Color.green, Time.deltaTime);

        // ===============================
        // RAYCAST TEST
        // ===============================
        if (GeometryRaycast.Raycast(ray, circle, out var hitC))
            GeometryRaycast.DrawGizmos(hitC, Color.cyan, Time.deltaTime);

        if (GeometryRaycast.Raycast(ray, box, out var hitB))
            GeometryRaycast.DrawGizmos(hitB, Color.yellow, Time.deltaTime);

        if (GeometryRaycast.Raycast(ray, seg, out var hitS))
            GeometryRaycast.DrawGizmos(hitS, Color.green, Time.deltaTime);

        // ===============================
        // CLOSEST POINT TEST
        // ===============================
        if (drawClosestPoint)
        {
            Vec2 p = new Vec2(rayOrigin.x, rayOrigin.y);

            Vec2 cpCircle = GeometryCircle.ClosestPoint(circle, p);
            Vec2 cpAABB = GeometryAABB.ClosestPoint(box, p);
            GeometrySegment.PointSegment(p, seg, out Vec2 cpSeg);

            DrawLine(p, cpCircle, Color.cyan);
            DrawLine(p, cpAABB, Color.yellow);
            DrawLine(p, cpSeg, Color.green);
        }
    }

    // ===============================
    // DRAW HELPERS
    // ===============================

    void DrawLine(Vec2 a, Vec2 b, Color c)
    {
        Gizmos.color = c;
        Gizmos.DrawLine(ToV3(a), ToV3(b));
    }

    Vector3 ToV3(Vec2 v) => new Vector3(v.x, v.y);
}
