using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

    public class Vertex
    {
        public Vector3 Pos;
        public Vector3 Normal;
        
        public Vertex(Vector3 pos, Vector3 normal = new())
        {
            Pos = pos;
            Normal = normal;
        }
    }

    private Mesh generatedMesh;
    List<Vertex> vertices = new();
    List<int> indices = new();

    // Start is called before the first frame update
    void Start()
    {
        InitMesh();
        CalculateNormals();


        var hit = GetCollision(new Vector2(15.19f, 15.19f));
        print($"Hit: {hit.isHit}");
        print($"Pos: {hit.Position}");
        print($"Norm: {hit.Normal}");
    }

    private void InitMesh()
    {
        vertices.Add(new Vertex(new Vector3(0, 21.6f, 0)));
        vertices.Add(new Vertex(new Vector3(56, 0, 0)));
        vertices.Add(new Vertex(new Vector3(0, 0, 56)));
        vertices.Add(new Vertex(new Vector3(56, 11, 56)));
        vertices.Add(new Vertex(new Vector3(112, 0, 56)));
        vertices.Add(new Vertex(new Vector3(112, 13, 0)));

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
            vertices = vertices.Select(v => v.Pos).ToArray(),
            triangles = indices.ToArray()
        };

        GetComponent<MeshFilter>().mesh = generatedMesh;
    }

    private void CalculateNormals()
    {
        for (var i = 0; i < indices.Count; i += 3)
        {
            int i1 = indices[i];
            int i2 = indices[i + 1];
            int i3 = indices[i + 2];

            var v1 = vertices[i1];
            var v2 = vertices[i2];
            var v3 = vertices[i3];
            
            var normal = Vector3.Cross(v2.Pos-v1.Pos, v3.Pos-v2.Pos).normalized;
            v1.Normal += normal;
            v2.Normal += normal;
            v3.Normal += normal;
            
        }

        vertices.ForEach(v => v.Normal = v.Normal.normalized);
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

            var v1 = vertices[i1];
            var v2 = vertices[i2];
            var v3 = vertices[i3];

            var v1e = new Vector2(v1.Pos.x, v1.Pos.z);
            var v2e = new Vector2(v2.Pos.x, v2.Pos.z);
            var v3e = new Vector2(v3.Pos.x, v3.Pos.z);

            float u, v, w;
            Barycentric(v1e, v2e, v3e, position, out u, out v, out w);

            if (u is >= 0f and <= 1f && v is >= 0f and <= 1f && w is >= 0f and <= 1f)
            {
                var y = vertices[i1].Pos.y * v + vertices[i2].Pos.y * u + vertices[i3].Pos.y * w;
                //print($"{vertices[i1].y} * {u} + {vertices[i2].y} * {v} + {vertices[i3].y} * {w} = {y}");
                hit.Position.y = y;
                //hit.Normal = v1.Normal;
                hit.Normal = Vector3.Cross(v2.Pos - v1.Pos, v3.Pos-v2.Pos).normalized;
                hit.isHit = true;
                return hit;
            }
        }
        return hit;
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

    void OnDrawGizmos()
    {
        // For each triangle
        for (var i = 0; i < indices.Count; i += 3)
        {
            int i1 = indices[i];
            var v1 = vertices[i1];
            Gizmos.DrawLine(v1.Pos, v1.Pos + (v1.Normal * 50));
        }
        
        Gizmos.DrawLine(new Vector3(15.69f, -500, 15.69f), new Vector3(15.69f, 500, 15.69f));
    }
}


