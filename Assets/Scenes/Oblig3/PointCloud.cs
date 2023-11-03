using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class ObjData
{
    public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;

    public Matrix4x4 matrix
    {
        get => Matrix4x4.TRS(pos, rot, scale);
    }
}

public class PointCloud : MonoBehaviour
{
    private class Vertex
    {
        public Vector3 Pos;

        public Vertex(Vector3 pos)
        {
            Pos = pos;
        }
    }
    
    /// <summary>
    /// Text file containing vertex data.
    /// </summary>
    [SerializeField] private TextAsset vertexData;
    /// <summary>
    /// The mesh the instanced objects will use.
    /// </summary>
    [SerializeField] private Mesh objMesh;
    /// <summary>
    /// The material the instanced objects will use.
    /// </summary>
    [SerializeField] private Material objMat;
    /// <summary>
    /// Point clouds contain millions of points, so we can skip some to improve performance.
    /// Describes how many lines to move each iteration when reading vertex data.
    /// </summary>
    [SerializeField] [Range(1, 100000)] int skipAmount = 100;
    /// <summary>
    /// Scale of the point cloud.
    /// </summary>
    [SerializeField] private float scale = 0.05f;
    
    private List<Vertex> vertices = new();
    private List<List<ObjData>> batches = new();
    
    /// <summary>
    /// Position offset for each point in the point cloud.
    /// Ensure that the point cloud is placed at the world origin. 
    /// </summary>
    private Vector3 offset;
    
    public ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private Bounds bounds = new Bounds(Vector3.zero, new Vector3(50.0f, 50.0f, 50.0f));


    void Start()
    {
        ReadFromFile();
        //SortDataIntoBatches();
        
        // Create a ComputeBuffer to store the mesh matrices
        int numInstances = vertices.Count;
        ComputeBuffer meshMatricesBuffer = new ComputeBuffer(numInstances, Marshal.SizeOf(typeof(Matrix4x4)));
        
        // Fill the ComputeBuffer with the mesh matrices
        for (int i = 0; i < numInstances; i++) {
            Matrix4x4 meshMatrix = Matrix4x4.identity;
            // ... calculate the mesh matrix for the i-th instance ...
            meshMatricesBuffer.SetData(new Matrix4x4[] { meshMatrix });
        }
        
        var argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = (uint)objMesh.GetIndexCount(0); // Index count per instance
        args[1] = (uint)objMat; // Instance count
        args[2] = (uint)instanceMesh.GetIndexStart(0); // Start index location
        args[3] = (uint)instanceMesh.GetBaseVertex(0); // Base vertex location
        argsBuffer.SetData(args);
    }

    /// <summary>
    /// Sort vertices into batches for rendering
    /// </summary>
    private void SortDataIntoBatches()
    {
        int batchIndexNum = 0;
        List<ObjData> currBatch = new();
        foreach (var i in vertices)
        {
            if (batchIndexNum == 1023)
            {
                batches.Add(currBatch);
                currBatch = new();
                batchIndexNum = 0;
            }
            
            currBatch.Add(new ObjData()
            {
                pos = i.Pos,
                scale = Vector3.one * 0.1f,
                rot = Quaternion.identity
            });
            
            batchIndexNum++;
        }
    }

    private void Update()
    {
        RenderBatches();
        RenderComputeBuffer();
    }

    private void RenderComputeBuffer()
    {
        
        
        Graphics.DrawMeshInstancedIndirect(Mesh, Material, SubmeshIndex, meshMatricesBuffer, indirectArgs);
    }

    /// <summary>
    /// Draws the all the mesh instances representing each point in the cloud.
    /// </summary>
    private void RenderBatches()
    {
        foreach (var batch in batches)
        {
            Graphics.DrawMeshInstanced(objMesh, 0, objMat, batch.Select((a) => a.matrix).ToList());
        }
    }

    /// <summary>
    /// Reads vertex and index data from file and puts them into the vertices and indices lists.
    /// </summary>
    /// <exception cref="FileNotFoundException">Throws if the data files are not set.</exception>
    private void ReadFromFile()
    {
        if (!vertexData) 
            throw new FileNotFoundException("Vertex data file not found.");
        
        // Delimiters we want to split on
        var delimfile = new[] {"\r\n", "\n", "\r"};
        var delimchars = new[] {' ', '\t'};
        
        // Split the text into lines
        var vertexLines = vertexData.text.Split(delimfile, System.StringSplitOptions.RemoveEmptyEntries);

        var vertexNumLines = int.Parse(vertexLines[0]);

        // Read and insert vertex data
        for (var i = 1; i < vertexNumLines - 100 + 1; i += skipAmount)
        {
            var xyz = vertexLines[i].Split(delimchars, StringSplitOptions.RemoveEmptyEntries);

            vertices.Add(new Vertex(new Vector3(
                float.Parse(xyz[0], CultureInfo.InvariantCulture), 
                float.Parse(xyz[1], CultureInfo.InvariantCulture), 
                float.Parse(xyz[2], CultureInfo.InvariantCulture)
            )));
        }
        
        // Set origin to first point
        offset = vertices[0].Pos;
        
        // Apply offset to all points and scale down
        foreach (var i in vertices)
        {
            i.Pos -= offset;
            (i.Pos.y, i.Pos.z) = (i.Pos.z, i.Pos.y);
            i.Pos *= scale;
        }
    }
}
