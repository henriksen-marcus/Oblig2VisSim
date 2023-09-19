using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BallPhysics : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] TriangleSurface triangleSurface;
    [SerializeField] [Range(0,1)] float bounciness = 0;

    [Header("Debug")] 
    [SerializeField] private bool showDebugSphere = true;
    
    private Vector3 g = Physics.gravity;
    private float m = 1f;
    private float r = 2f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 prevNormal = Vector3.zero;
    private Vector3 lastGivenPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = 0.1f;
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
        
        bool validY = Mathf.Abs(hit.Position.y - pos.y) <= r;
        
        if (hit.isHit && validY)
        {
            normalVelocity = Vector3.Dot(velocity, hit.Normal) * hit.Normal;
            
            // Reflection
            velocity = velocity - normalVelocity - bounciness * normalVelocity;
            
            lastGivenPos = hit.Position;
            
            N = -Vector3.Dot(hit.Normal, G) * hit.Normal;

            /*print("Pos:" + hit.Position);
            print("Norm" + hit.Normal);*/
            //print("Hit");
        }
        else lastGivenPos = Vector3.zero;

        Vector3 accel;
        accel = (G + N) / m;

        velocity += accel * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Modifies the hit position to accurately reflect the ball's
    /// position if it were to lie on the triangle surface.
    /// </summary>
    /// <param name="hit"></param>
    void CorrectCollisionToSurface(ref TriangleSurface.Hit hit)
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
}
