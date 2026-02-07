using Geometry;
using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;
using Ray = Geometry.Ray;

public class GeometryTest : MonoBehaviour
{
    [Header("SHAPE")] public bool enableShape;
    public Shape a;
    public float2 aPrefPos;
    public float2 aPos;
    public float aAngle;

    public Shape b;
    public float2 bPos;
    public float bAngle;

    [Header("RAY")] public bool enableRay;
    public float2 cPos;
    public float2 cDir;
    public float cLength;

    private void OnDrawGizmos()
    {
        // ========================== GIZMOS ========================== //

        if (enableShape)
        {
            switch (a.type)
            {
                case ShapeType.Box:
                    if (math.abs(aAngle) <= 0)
                    {
                        GeometryGizmos.DrawAABB(
                            AABB.FromCenter(aPrefPos, new float2(0.15f, 0.15f)),
                            new Color(1, 1, 0, 0.15f), Time.deltaTime
                        );
                        GeometryGizmos.DrawAABB(
                            AABB.FromCenter(aPrefPos, a.size),
                            new Color(1, 1, 0, 0.15f), Time.deltaTime
                        );
                        GeometryGizmos.DrawAABB(
                            AABB.FromCenter(aPos, a.size),
                            new Color(1, 1, 0, 0.35f), Time.deltaTime
                        );
                        Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                            new Color(1, 1, 0, 0.35f), Time.deltaTime
                        );
                    }
                    else
                    {
                        GeometryGizmos.DrawAABB(
                            AABB.FromCenter(aPrefPos, new float2(0.15f, 0.15f)),
                            new Color(1, 1, 0, 0.15f), Time.deltaTime
                        );
                        GeometryGizmos.DrawOBB(
                            new OBB(aPrefPos, a.size / 2, Mathf.Deg2Rad * aAngle),
                            new Color(1, 1, 0, 0.15f), Time.deltaTime
                        );
                        GeometryGizmos.DrawOBB(
                            new OBB(aPos, a.size / 2, Mathf.Deg2Rad * aAngle),
                            new Color(1, 1, 0, 0.35f), Time.deltaTime
                        );
                        Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                            new Color(1, 1, 0, 0.35f), Time.deltaTime
                        );
                    }

                    break;
                case ShapeType.Circle:
                    GeometryGizmos.DrawAABB(
                        AABB.FromCenter(aPrefPos, new float2(0.15f, 0.15f)),
                        new Color(1, 1, 0, 0.15f), Time.deltaTime
                    );
                    GeometryGizmos.DrawCircle(
                        new Circle(aPrefPos, a.radius),
                        new Color(1, 1, 0, 0.15f), Time.deltaTime
                    );
                    GeometryGizmos.DrawCircle(
                        new Circle(aPos, a.radius),
                        new Color(1, 1, 0, 0.35f), Time.deltaTime
                    );
                    Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                        new Color(1, 1, 0, 0.35f), Time.deltaTime
                    );
                    break;
            }
        }
        
        switch (b.type)
        {
            case ShapeType.Box:
                if (math.abs(bAngle) <= 0)
                {
                    GeometryGizmos.DrawAABB(AABB.FromCenter(bPos, b.size),
                        new Color(0, 1, 1, 0.35f), Time.deltaTime
                    );
                }
                else
                {
                    GeometryGizmos.DrawOBB(new OBB(bPos, b.size * 0.5f, Mathf.Deg2Rad * bAngle),
                        new Color(0, 1, 1, 0.35f), Time.deltaTime
                    );
                }

                break;
            case ShapeType.Circle:
                GeometryGizmos.DrawCircle(new Circle(bPos, b.radius),
                    new Color(0, 1, 1, 0.35f), Time.deltaTime
                );
                break;
        }

        if (enableRay)
        {
            GeometryGizmos.DrawAABB(
                AABB.FromCenter(cPos, new float2(0.5f, 0.5f)),
                new Color(0, 1, 0, 0.35f), Time.deltaTime
            );
            GeometryGizmos.DrawRay(
                new Ray(cPos, cDir),
                new Color(0, 1, 0, 0.35f), Time.deltaTime,
                cLength
            );
        }

        // ========================== SWEEP ========================== //

        if (enableShape)
        {
            if (a.type == ShapeType.Circle && b.type == ShapeType.Circle)
            {
                GeometrySweep.SweepCircleCircle(
                    aPrefPos, aPos, a.radius,
                    new Circle(bPos, b.radius), out var hit2D
                );
                if (hit2D.hit)
                {
                    GeometryGizmos.DrawCircle(new Circle(hit2D.point, a.radius),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                    GeometryGizmos.DrawAABB(AABB.FromCenter(hit2D.point, new float2(0.15f, 0.15f)),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                }
                else
                {
                    Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                        new Color(1, 0, 1, 0.35f), Time.deltaTime
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
                    GeometryGizmos.DrawCircle(new Circle(hit2D.point, a.radius),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                    GeometryGizmos.DrawAABB(AABB.FromCenter(hit2D.point, new float2(0.15f, 0.15f)),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                }
                else
                {
                    Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                        new Color(1, 0, 1, 0.35f), Time.deltaTime
                    );
                }
            }
            else if (a.type == ShapeType.Box && b.type == ShapeType.Circle)
            {
                RayHit2D hit2D;
                if (math.abs(bAngle) <= 0)
                {
                    GeometrySweep.SweepAABBCircle(aPrefPos, aPos, a.size * 0.5f,
                        new Circle(bPos, b.radius), out hit2D
                    );
                }
                else
                {
                    GeometrySweep.SweepAABBCircle(aPrefPos, aPos, a.size * 0.5f,
                        new Circle(bPos, b.radius), out hit2D
                    );
                }

                if (hit2D.hit)
                {
                    GeometryGizmos.DrawAABB(AABB.FromCenter(hit2D.point, a.size),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                    GeometryGizmos.DrawAABB(AABB.FromCenter(hit2D.point, new float2(0.15f, 0.15f)),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                }
                else
                {
                    Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                        new Color(1, 0, 1, 0.35f), Time.deltaTime
                    );
                }
            }
            else if (a.type == ShapeType.Box && b.type == ShapeType.Box)
            {
                RayHit2D hit2D;
                if (math.abs(aAngle) <= 0 && math.abs(bAngle) <= 0)
                {
                    GeometrySweep.SweepAABBAABB(aPrefPos, aPos, a.size * 0.5f,
                        AABB.FromCenter(bPos, b.size), out hit2D
                    );
                }
                else if (math.abs(aAngle) <= 0 && math.abs(bAngle) > 0)
                {
                    GeometrySweep.SweepAABBOBB(aPrefPos, aPos, a.size * 0.5f,
                        new OBB(bPos, b.size * 0.5f, Mathf.Deg2Rad * bAngle), out hit2D
                    );
                }
                else
                {
                    GeometrySweep.SweepAABBAABB(aPrefPos, aPos, a.size * 0.5f,
                        AABB.FromCenter(bPos, b.size), out hit2D
                    );
                }

                if (hit2D.hit)
                {
                    GeometryGizmos.DrawAABB(AABB.FromCenter(hit2D.point, a.size),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                    GeometryGizmos.DrawAABB(AABB.FromCenter(hit2D.point, new float2(0.15f, 0.15f)),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                }
                else
                {
                    Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                        new Color(1, 0, 1, 0.35f), Time.deltaTime
                    );
                }
            }
        }

        if (enableRay)
        {
            if (b.type == ShapeType.Circle)
            {
                GeometryRaycast.Raycast(
                    new Ray(cPos, cDir), cLength,
                    new Circle(bPos, b.radius),
                    out var hit2D
                );

                if (hit2D.hit)
                {
                    GeometryGizmos.DrawAABB(
                        AABB.FromCenter(hit2D.point, new float2(0.5f, 0.5f)),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                }
                else
                {
                    Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                        new Color(1, 0, 1, 0.35f), Time.deltaTime
                    );
                }
            }
            else if (b.type == ShapeType.Box)
            {
                RayHit2D hit2D;

                if (math.abs(aAngle) <= 0)
                {
                    GeometryRaycast.Raycast(
                        new Ray(cPos, cDir), cLength,
                        AABB.FromCenter(bPos, b.size),
                        out hit2D
                    );                    
                }
                else
                {
                    GeometryRaycast.Raycast(
                        new Ray(cPos, cDir), cLength,
                        AABB.FromCenter(bPos, b.size),
                        out hit2D
                    ); 
                    /*GeometryRaycast.Raycast(
                        new Ray(cPos, cDir), cLength,
                        new OBB(bPos, b.size), (bPos, b.size),
                        out hit2D
                    ); */
                }

                if (hit2D.hit)
                {
                    GeometryGizmos.DrawAABB(
                        AABB.FromCenter(hit2D.point, new float2(0.5f, 0.5f)),
                        new Color(1, 0, 0, 0.35f), Time.deltaTime
                    );
                }
                else
                {
                    Debug.DrawLine((Vector2) aPrefPos, (Vector2) aPos,
                        new Color(1, 0, 1, 0.35f), Time.deltaTime
                    );
                }
            }
        }
    }
}