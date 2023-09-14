using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings.SplashScreen;

public class TriangleSurface : MonoBehaviour
{
    /// <summary>
    /// Information about a potential hit with the triangle
    /// mesh surface.
    /// </summary>
    public struct Hit
    {
        /// <summary>
        /// Collision point in world space.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Normal on the surface at the point of contact. Is normalized.
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// If there was a collision.
        /// </summary>
        public bool isHit;
    }


    private Mesh generatedMesh;

    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();
    List<Vector3> tri_indices = new List<Vector3>();

    

    // Start is called before the first frame update
    void Start()
    {
        InitMesh();
        
        var a = GetCollision(new Vector2(5, 5));
        if(a.isHit) print(a.Position);
    }

    private void InitMesh()
    {
        vertices.Add(new Vector3(0, 21.6f, 0));
        vertices.Add(new Vector3(56, 0, 0));
        vertices.Add(new Vector3(0, 0, 56));
        vertices.Add(new Vector3(56, 11, 56));
        vertices.Add(new Vector3(112, 0, 56));
        vertices.Add(new Vector3(112, 13, 0));

        // 1     With the clock
        // |\
        // | \
        // |  \
        // 0___2

        indices.Add(0);
        indices.Add(2);
        indices.Add(1);

        indices.Add(1);
        indices.Add(2);
        indices.Add(3);

        indices.Add(3);
        indices.Add(4);
        indices.Add(1);

        indices.Add(4);
        indices.Add(5);
        indices.Add(1);

        generatedMesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = indices.ToArray()
        };

        GetComponent<MeshFilter>().mesh = generatedMesh;
    }

    public Hit GetCollision(Vector2 position)
    {
        var hit = new Hit();
        hit.Position.x = position.x;
        hit.Position.z = position.y;

        for (var i = 0; i < indices.Count; i += 3)
        {
            int i1 = indices[i];
            int i2 = indices[i + 1];
            int i3 = indices[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            float u, v, w;
            Barycentric(v1, v2, v3, position, out u, out v, out w);

            if (u is >= 0f and <= 1f && v is >= 0f and <= 1f && w is >= 0f and <= 1f)
            {
                var y = vertices[i1].y * v + vertices[i2].y * u + vertices[i3].y * w;
                //print($"{vertices[i1].y} * {u} + {vertices[i2].y} * {v} + {vertices[i3].y} * {w} = {y}");
                hit.Position.y = y;
                hit.Normal = Vector3.Cross(v1, v2).normalized;
                hit.isHit = true;
                return hit;
            }
        }
        return hit;
    }

    public float GetHeight(Vector2 point)
    {
        for (int i = 0; i < indices.Count; i += 3)
        {
            int i1 = indices[i];
            int i2 = indices[i + 1];
            int i3 = indices[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            float u, v, w;
            Barycentric(v1, v2, v3, point, out u, out v, out w);

            if (u >= 0f && u <= 1f && v >= 0f && v <= 1f && w >= 0f && w <= 1f)
            {
                float h = vertices[i1].y * u + vertices[i2].y * v + vertices[i3].y * w;
                return h;
            }
        }

        return -1;
    }

    public static void Barycentric(Vector2 a, Vector2 b, Vector2 c, Vector2 p, out float u, out float v, out float w)
    {
        Vector2 v0 = b - a;
        Vector2 v1 = c - a;
        Vector2 v2 = p - a;

        float d00 = Vector2.Dot(v0, v0);
        float d01 = Vector2.Dot(v0, v1);
        float d11 = Vector2.Dot(v1, v1);
        float d20 = Vector2.Dot(v2, v0);
        float d21 = Vector2.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;

        v = (d11 * d20 - d01 * d21) / denom;
        w = (d00 * d21 - d01 * d20) / denom;
        u = 1.0f - v - w;
    }
}
