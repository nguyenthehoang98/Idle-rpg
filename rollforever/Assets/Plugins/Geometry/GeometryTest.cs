using Geometry;
using Geometry.Primary;
using Unity.Mathematics;
using UnityEngine;

public class GeometryTest : MonoBehaviour
{
    public bool enableShape;
    
    [Header("SHAPE")] 
    public Shape a;
    public float2 aPrevPos;
    public float2 aPos;
    public float aAngle;

    public Shape b;
    public float2 bPos;
    public float bAngle;

    private void OnDrawGizmos()
    {
        if (enableShape)
        {
            bool hit = GeometryUtils.Overlaps(
                a, aPrevPos, aPos, b, bPos
            );
            
            Color color = Color.gray;
            if (hit)
            {
                color = Color.red;
            }
            else
            {
                hit = GeometryUtils.Sweep(
                    a, aPrevPos, aPos, b, bPos
                );

                if (hit) color = Color.magenta;
            }
            
            GeometryGizmos.DrawShape(a, aPrevPos, color, Time.deltaTime);
            GeometryGizmos.DrawShape(a, aPos, color, Time.deltaTime);
            GeometryGizmos.DrawShape(b, bPos, color, Time.deltaTime);
        }
    }
}