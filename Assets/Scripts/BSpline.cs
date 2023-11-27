using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BSpline : MonoBehaviour
{
    /// <summary>
    /// Number of control points.
    /// </summary>
    private int n;
    /// <summary>
    /// Degree of the curve.
    /// </summary>
    private int d = 2;
    List<int> knotVector = new List<int>();
    [SerializeField] List<Vector3> controlPoints = new();
    [SerializeField] private int resolutionPerPoint = 8;

    void Start()
    {
        n = controlPoints.Count;
        UpdateKnotVector();
        Evaluate(1);
    }

    void Update()
    {
        /*for (float i = 1; i <= resolution; i++)
        {
            Debug.DrawLine(Evaluate((i-1)/resolution), Evaluate(i/resolution));
        }*/
    }
    
    public void AddPoint(Vector3 point)
    {
        controlPoints.Add(point);
        n = controlPoints.Count;
        UpdateKnotVector();
    }
    
    public int GetCount() => controlPoints.Count;

    public Vector3[] GetPoints()
    {
        int resolution = resolutionPerPoint * (n - 1);
        Vector3[] points = new Vector3[resolution+1];

        for (int i = 0; i <= resolution; i++)
        {
            points[i] = Evaluate((float)i / resolution);
        }

        return points;
    }

    private int findKnotInverval(float x)
    {
        int my = n - 1;
        while (x < knotVector[my]) my--;
        return my;
    }

    /// <summary>
    /// Clears and updates the knot vector to the correct
    /// values for the current number of control points and degree.
    /// </summary>
    private void UpdateKnotVector()
    {
        knotVector.Clear();
        for (int i = 0; i < n + d + 1; i++)
        {
            if (i < d + 1)
                knotVector.Add(0);
            else if (i > n)
                knotVector.Add(n-d);
            else
                knotVector.Add(i-d);
        }
    }

    /// <param name="t">Time along curve. Ranges 0-1f.</param>
    /// <returns>The position of the curve at time t.</returns>
    private Vector3 Evaluate(float t)
    {
        if (n < 3)
        {
            print("Not enough control points to evaluate spline!");
            return Vector3.zero;
        }
        
        float range = n - 2;
        t = Mathf.Clamp01(t) * range;
        
        // Find the last knot vector that is smaller than t
        int my = findKnotInverval(t);

        List<Vector3> affectedControlPoints = new List<Vector3>(d + 1);
        
        // Resize a to d + 1
        for (int i = 0; i <= d; i++)
            affectedControlPoints.Add(Vector3.zero);

        // Add the d+1 control points that are affected by t
        for (int j = 0; j <= d; j++)
            affectedControlPoints[d - j] = controlPoints[my - j];

        for (int i = d; i > 0; i--)
        {
            int knotIndex = my - i;
            for (int j = 0; j < i; j++)
            {
                knotIndex++;
                // Calculate w_i,d(t) using the general formula
                float w = (t - knotVector[knotIndex]) / (knotVector[knotIndex + i] - knotVector[knotIndex]);
                affectedControlPoints[j] = affectedControlPoints[j] * (1 - w) + affectedControlPoints[j + 1] * w;
            }
        }

        return affectedControlPoints[0];
    }

    public void Reset()
    {
        controlPoints.Clear();
        n = 0;
    }

    /*private void OnDrawGizmos()
    {
        foreach (var v in controlPoints)
            Gizmos.DrawWireSphere(v, 0.5f);
    }*/
}
