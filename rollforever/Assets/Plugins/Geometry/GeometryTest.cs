using Geometry;
using Unity.Mathematics;
using UnityEngine;

public class GeometryTest : MonoBehaviour
{
    public Shape a;
    public float2 aPrefPos;
    public float2 aPos;
    public float aAngle;

    public Shape b;
    public float2 bPos;
    public float bAngle;

    private void OnDrawGizmos()
    {
        switch (a.type)
        {
            case ShapeType.Box:
                if (math.abs(aAngle) <= 0)
                {
                    GeometryGizmos.DrawAABB(
                        AABB.FromCenter(aPrefPos, a.size / 2),
                        new Color(1, 1, 0, 0.05f), Time.deltaTime
                    );
                    GeometryGizmos.DrawAABB(
                        AABB.FromCenter(aPos, a.size / 2),
                        new Color(1, 1, 0, 0.5f), Time.deltaTime
                    );
                }
                else
                {
                    GeometryGizmos.DrawOBB(
                        new OBB(aPrefPos, a.size / 2, Mathf.Deg2Rad * aAngle),
                        new Color(1, 1, 0, 0.05f), Time.deltaTime
                    );
                    GeometryGizmos.DrawOBB(
                        new OBB(aPos, a.size / 2, Mathf.Deg2Rad * aAngle),
                        new Color(1, 1, 0, 0.5f), Time.deltaTime
                    );
                }

                Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos, Color.yellow, Time.deltaTime);
                break;
            case ShapeType.Circle:
                GeometryGizmos.DrawCircle(
                    new Circle(aPrefPos, a.radius),
                    new Color(1, 1, 0, 0.05f), Time.deltaTime
                );
                GeometryGizmos.DrawCircle(
                    new Circle(aPos, a.radius),
                    new Color(1, 1, 0, 0.5f), Time.deltaTime
                );
                break;
        }

        switch (b.type)
        {
            case ShapeType.Box:
                if (math.abs(bAngle) <= 0)
                {
                    GeometryGizmos.DrawAABB(
                        AABB.FromCenter(bPos, b.size),
                        new Color(0, 1, 1, 0.5f), Time.deltaTime
                    );
                }
                else
                {
                    GeometryGizmos.DrawOBB(
                        new OBB(bPos, b.size * 0.5f, Mathf.Deg2Rad * bAngle),
                        new Color(0, 1, 1, 0.5f), Time.deltaTime
                    );                    
                }
                break;
            case ShapeType.Circle:
                GeometryGizmos.DrawCircle(
                    new Circle(bPos, b.radius), new Color(0, 1, 1, 0.5f), Time.deltaTime
                );
                break;
        }

        if (a.type == ShapeType.Circle && b.type == ShapeType.Circle)
        {
            GeometrySweep.SweepCircleCircle(
                aPrefPos, aPos, a.radius,
                new Circle(bPos, b.radius), out var hit2D
            );
            if (hit2D.hit)
            {
                GeometryGizmos.DrawCircle(new Circle(hit2D.point, b.radius),
                    new Color(1, 0, 0, 1), Time.deltaTime
                );
            }
            else
            {
                Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                    new Color(1, 1, 0, 1), Time.deltaTime
                );
            }
        }
        else if (a.type == ShapeType.Circle && b.type == ShapeType.Box)
        {
            RayHit2D hit2D;
            if (math.abs(bAngle) <= 0)
            {
                GeometrySweep.SweepCircleAABB(aPrefPos, aPos, a.radius,
                    AABB.FromCenter(bPos, b.size), out hit2D
                );
            }
            else
            {
                GeometrySweep.SweepCircleOBB(aPrefPos, aPos, a.radius,
                    new OBB(bPos, b.size * 0.5f, Mathf.Deg2Rad * bAngle), out hit2D
                );
            }
            
            if (hit2D.hit)
            {
                GeometryGizmos.DrawCircle(new Circle(hit2D.point, b.radius),
                    new Color(1, 0, 0, 1), Time.deltaTime
                );
            }
            else
            {
                Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                    new Color(1, 1, 0, 1), Time.deltaTime
                );
            }
        }
    }
}
