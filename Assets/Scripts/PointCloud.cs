using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    [SerializeField] private float pointScale = 0.5f;
    
    private List<Vector3> vertices = new();
    private List<List<ObjData>> batches = new();

    private GameObject player;
    private Vector3 pos;
    
    /// <summary>
    /// Position offset for each point in the point cloud.
    /// Ensure that the point cloud is placed at the world origin. 
    /// </summary>
    private Vector3 offset;

    void Start()
    {
        ReadFromFile();

        // Sort vertices into batches for rendering
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
                pos = i,
                scale = Vector3.one * pointScale,
                rot = Quaternion.identity
            });
            
            batchIndexNum++;
        }
        
        batches.Add(currBatch);
    }

    private void Update()
    {
        RenderBatches();
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
        for (var i = 1; i < vertexNumLines; i += skipAmount)
        {
            var xyz = vertexLines[i].Split(delimchars, StringSplitOptions.RemoveEmptyEntries);

            vertices.Add(new Vector3(
                float.Parse(xyz[0], CultureInfo.InvariantCulture), 
                float.Parse(xyz[1], CultureInfo.InvariantCulture), 
                float.Parse(xyz[2], CultureInfo.InvariantCulture)
            ));
        }
        
        // Set origin to first point
        offset = vertices[0];
        
        // Apply offset to all points and scale down
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] *= scale;
        }
    }

    private void OnDrawGizmos()
    {
    }
}
