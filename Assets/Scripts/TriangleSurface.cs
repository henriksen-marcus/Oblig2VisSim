using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

    private class Vertex
    {
        public Vector3 Pos;
        public Vector3 Normal;
        
        public Vertex(Vector3 pos, Vector3 normal = new())
        {
            Pos = pos;
            Normal = normal;
        }
    }

    [SerializeField] private TextAsset vertexData;
    [SerializeField] private TextAsset indexData;
    [SerializeField] private bool drawMeshLines = false;
    [SerializeField] private Color lineColor = Color.grey;
    
    private Mesh generatedMesh;
    List<Vertex> vertices = new();
    List<int> indices = new();

    private Vector3 temp;

    // Start is called before the first frame update
    void Start()
    {
        InitMesh();
        //ReadFromFile();
    }
    
    /// <summary>
    /// Reads vertex and index data from file and puts them into the vertices and indices lists.
    /// </summary>
    /// <exception cref="FileNotFoundException">Throws if the data files are not set.</exception>
    private void ReadFromFile()
    {
        if (!vertexData || !indexData) 
            throw new FileNotFoundException("Vertex or index data files not found.");
        
        // Delimiters we want to split on
        var delimfile = new[] {"\r\n", "\n", "\r"};
        var delimchars = new[] {' ', '\t'};
        
        // Split the text into lines
        var vertexLines = vertexData.text.Split(delimfile, System.StringSplitOptions.RemoveEmptyEntries);
        var indexLines = indexData.text.Split(delimfile, System.StringSplitOptions.RemoveEmptyEntries);

        var vertexNumLines = int.Parse(vertexLines[0]);
        var indexNumLines = int.Parse(indexLines[0]);
        
        // Read and insert vertex data
        for (var i = 1; i < vertexNumLines + 1; i++)
        {
            var xyz = vertexLines[i].Split(delimchars, StringSplitOptions.RemoveEmptyEntries);

            vertices.Add(new Vertex(new Vector3(
                float.Parse(xyz[0], CultureInfo.InvariantCulture), 
                float.Parse(xyz[1], CultureInfo.InvariantCulture), 
                float.Parse(xyz[2], CultureInfo.InvariantCulture)
            )));
        }
        
        // Read and insert index data
        for (var i = 1; i < indexNumLines + 1; i++)
        {
            var line = indexLines[i].Split(delimchars, StringSplitOptions.RemoveEmptyEntries);
            
            indices.Add(int.Parse(line[0]));
            indices.Add(int.Parse(line[1]));
            indices.Add(int.Parse(line[2]));
        }

        temp = vertices[0].Pos;
    }

    private void InitMesh()
    {
        // 1     With the clock
        // |\
        // | \
        // |  \
        // 0___2
        
        ReadFromFile();

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
                var y = vertices[i1].Pos.y * u + vertices[i2].Pos.y * v + vertices[i3].Pos.y * w;
                //print($"{vertices[i1].y} * {u} + {vertices[i2].y} * {v} + {vertices[i3].y} * {w} = {y}");
                hit.Position.y = y;
                //hit.Normal = v1.Normal;
                hit.Normal = Vector3.Cross(v2.Pos - v1.Pos, v3.Pos-v2.Pos).normalized;
                hit.isHit = true;
                //print("Triangle: " + i/3 + " hit!");
                return hit;
            }
        }
        return hit;
    }

    private static void Barycentric(Vector2 a, Vector2 b, Vector2 c, Vector2 p, out float u, out float v, out float w)
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
        if (!drawMeshLines) return;
        
        // Draw normals for each triangle
        /*for (var i = 0; i < indices.Count; i += 3)
        {
            int i1 = indices[i];
            int i2 = indices[i + 1];
            int i3 = indices[i + 2];

            var v1 = vertices[i1];
            var v2 = vertices[i2];
            var v3 = vertices[i3];

            var normal = Vector3.Cross(v2.Pos - v1.Pos, v3.Pos - v2.Pos).normalized * 0.1f;
            Gizmos.color = UnityEngine.Color.cyan;
            Gizmos.DrawLine(v1.Pos, v1.Pos + normal);
            Gizmos.DrawLine(v2.Pos, v2.Pos + normal);
            Gizmos.DrawLine(v3.Pos, v3.Pos + normal);
        }*/
        
        // Draw line around each triangle
        for (var i = 0; i < indices.Count; i += 3)
        {
            int i1 = indices[i];
            int i2 = indices[i + 1];
            int i3 = indices[i + 2];

            var v1 = vertices[i1];
            var v2 = vertices[i2];
            var v3 = vertices[i3];

            Gizmos.color = lineColor;
            Gizmos.DrawLine(v1.Pos, v2.Pos);
            Gizmos.DrawLine(v2.Pos, v3.Pos);
            Gizmos.DrawLine(v3.Pos, v1.Pos);
        }
    }
}


