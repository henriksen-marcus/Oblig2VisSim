using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Range(0,1)] float bounciness = 0;
    [SerializeField] private bool drawSplinePath = false;
    [SerializeField] private float distanceBetweenSplinePoints = 15f;
    
    [Header("Debug")] 
    [SerializeField] private bool showDebugSphere = true;

    /// <summary>
    /// Gravitational acceleration.
    /// </summary>
    private Vector3 g = Physics.gravity;
    /// <summary>
    /// Mass.
    /// </summary>
    public float m { get; private set; }= 1f;
    /// <summary>
    /// Radius.
    /// </summary>
    private float r;
    /// <summary>
    /// Sphere trigger that checks for overlapping balls.
    /// </summary>
    private SphereCollider trigger;
    public Vector3 velocity {get; private set;} = Vector3.zero;
    private Vector3 lastCollPosition = Vector3.zero;
    private List<BallPhysics> ballIgnoreList = new();
    private BSpline splinePath;
    private Vector3 lastSplinePoint;
    private LineRenderer lineRenderer;
    private TriangleSurface triangleSurface;
    private bool hasBegunTracingPath = false;

    // Start is called before the first frame update
    void Start()
    {
        if (triangleSurface == null)
            triangleSurface = WorldManager.Instance.triangleSurface;
        
        // Initialize radius
        r = transform.localScale.x / 2;

        trigger = GetComponent<SphereCollider>();
        if (trigger == null)
            print("SHIT");
        
        splinePath = GetComponent<BSpline>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        Vector2 pos2D = new Vector2(pos.x, pos.z);
        
        // Get collision information
        var hit = triangleSurface.GetCollision(pos2D);
        CorrectCollisionToSurface(ref hit);
        
        Vector3 N = new Vector3();
        Vector3 G = m * g;
        Vector3 normalVelocity;
        
        // Check if we are at the same height as the surface
        float dist = Vector3.Distance(hit.Position, pos);
        bool validY = dist <= r;
        
        if (hit.isHit && validY)
        {
            normalVelocity = Vector3.Dot(velocity, hit.Normal) * hit.Normal;
            // Reflection
            velocity = velocity - normalVelocity - bounciness * normalVelocity;
            lastCollPosition = hit.Position;
            N = -Vector3.Dot(hit.Normal, G) * hit.Normal;
            
            // Move ball up to surface
            transform.position = hit.Position + r * hit.Normal;
            
            // Enable this once we hit the surface
            hasBegunTracingPath = true;
        }
        else lastCollPosition = Vector3.zero;

        var accel = (G + N) / m;

        velocity += accel * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
        
        //print($"Position: {transform.position}\nVelocity: {velocity}\nAcceleration: {accel}\nNormal: {hit.Normal}");
    }

    private void Update()
    {
        CheckForBallCollision();
        
        if (!hasBegunTracingPath || !drawSplinePath) return;
        
        // Add position to spline path
        if (lastSplinePoint == Vector3.zero)
        {
            lastSplinePoint = transform.position;
            splinePath.AddPoint(lastSplinePoint);
        }
        // Only add point if we have moved a significant distance, to avoid wasting performance
        else if (Vector3.Distance(lastSplinePoint, transform.position) > distanceBetweenSplinePoints)
        {
            lastSplinePoint = transform.position;
            splinePath.AddPoint(lastSplinePoint);
            
            // Update line renderer if we have enough points
            int count = splinePath.GetCount();
            if (count > 2)
            {
                var points = splinePath.GetPoints();
                lineRenderer.positionCount = points.Length;
                lineRenderer.SetPositions(points);
            }
        }
    }

    /// <summary>
    /// Modifies the hit position to accurately reflect the ball's
    /// position if it were to lie on the triangle surface.
    /// </summary>
    private void CorrectCollisionToSurface(ref TriangleSurface.Hit hit)
    {
        Vector3 p = transform.position;
        Vector3 d = p - hit.Position;
        hit.Position = p - Vector3.Dot(d, hit.Normal) * hit.Normal;
    }

    void CheckForBallCollision()
    {
        // We need to use OverlapSphere because OnTriggerEnter requires a rigidbody
        Collider[] results = new Collider[5];
        var size = Physics.OverlapSphereNonAlloc(transform.position, r, results);

        if (size == 1) return;

        // Check if we are overlapping any balls
        for (int i = 0; i < size; i++)
        {
            var ball = results[i].GetComponent<BallPhysics>();
            if (ball == null || ball == this || ballIgnoreList.Contains(ball)) continue;

            // Our momentum, P = mv
            Vector3 P = m * velocity;
            // Momentum of other ball
            Vector3 P_o = ball.m * ball.velocity;

            // Exchange energy/momentum
            var momentumExchange = P_o * 0.3f;
            velocity += momentumExchange;
            ball.velocity -= momentumExchange;
            //Debug.DrawLine(transform.position, ball.transform.position, Color.red, 0.5f);
            ballIgnoreList.Clear();
            ballIgnoreList.Add(ball);
        }
    }

    /*private void OnDrawGizmos()
    {
        if (showDebugSphere)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastCollPosition, 2.2f);
        }
    }*/
    
    public void Reset()
    {
        velocity = Vector3.zero;
        lastCollPosition = Vector3.zero;
        lastSplinePoint = Vector3.zero;
        splinePath.Reset();
        lineRenderer.positionCount = 0;
        hasBegunTracingPath = false;
    }
}
