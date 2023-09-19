using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BallPhysics : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float mass = 1f;
    [SerializeField] TriangleSurface triangleSurface;
    [SerializeField] [Range(0,1)] float bounciness = 0;

    [Header("Debug")] 
    [SerializeField] private bool showDebugSphere;
    
    private Vector3 g = Physics.gravity;
    private float m = 1f;
    private float r = 2f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 prevNormal = Vector3.zero;
    private Vector3 lastGivenPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = 0.3f;
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
        
        bool validY = Mathf.Abs(hit.Position.y - pos.y) <= r;
        

        if (hit.isHit /*&& validY*/)
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

    private void OnDrawGizmos()
    {
        if (showDebugSphere)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastGivenPos, 4f);
        }
    }
}
