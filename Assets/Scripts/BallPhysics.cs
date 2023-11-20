using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Animations;

public class BallPhysics : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Range(0,1)] float bounciness = 0;
    
    [Header("Debug")] 
    [SerializeField] private bool showDebugSphere = true;

    public TriangleSurface triangleSurface;
    
    private Vector3 g = Physics.gravity;
    private float m = 1f;
    private float r = 0.02f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 prevNormal = Vector3.zero;
    private Vector3 lastGivenPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (triangleSurface == null)
            triangleSurface = WorldManager.Instance.triangleSurface;
        
        r = transform.localScale.x / 2;
    }

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        Vector2 pos2D = new Vector2(pos.x, pos.z);
        var hit = triangleSurface.GetCollision(pos2D);

        Vector3 newVel = velocity;
        Vector3 N = new Vector3();
        Vector3 G = m * g;
        Vector3 normalVelocity;
        
        CorrectCollisionToSurface(ref hit);
        
        float dist = Vector3.Distance(hit.Position, pos);
        bool validY = dist <= r;
        
        if (hit.isHit && validY)
        {
            normalVelocity = Vector3.Dot(velocity, hit.Normal) * hit.Normal;
            
            // Reflection
            velocity = velocity - normalVelocity - bounciness * normalVelocity;
            
            lastGivenPos = hit.Position;
            
            N = -Vector3.Dot(hit.Normal, G) * hit.Normal;

            // Move ball up to surface
            transform.position = hit.Position + r * hit.Normal;
        }
        else lastGivenPos = Vector3.zero;

        var accel = (G + N) / m;

        velocity += accel * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
        
        //print($"Position: {transform.position}\nVelocity: {velocity}\nAcceleration: {accel}\nNormal: {hit.Normal}");
    }

    /// <summary>
    /// Modifies the hit position to accurately reflect the ball's
    /// position if it were to lie on the triangle surface.
    /// </summary>
    private void CorrectCollisionToSurface(ref TriangleSurface.Hit hit)
    {
        var c = transform.position;
        Vector3 d = c - hit.Position;
        hit.Position = c - Vector3.Dot(d, hit.Normal) * hit.Normal;
    }

    private void OnDrawGizmos()
    {
        if (showDebugSphere)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastGivenPos, 2.2f);
        }
    }
    
    public void Reset()
    {
        velocity = Vector3.zero;
        prevNormal = Vector3.zero;
        lastGivenPos = Vector3.zero;
    }
}
