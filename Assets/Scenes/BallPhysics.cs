using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BallPhysics : MonoBehaviour
{
    [SerializeField] TriangleSurface triangleSurface;
    Vector3 g = Physics.gravity;
    float m = 1f;
    float r = 2f;
    Vector3 velocity = Vector3.zero;
    Vector3 prevNormal = Vector3.zero;
    [SerializeField] [Range(0,1)] float bounciness = 0; 

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

        //print($"isHit: {hit.isHit}, Pos: {hit.Position}, ValidY: {validY}");

        if (hit.isHit /*&& validY*/)
        {
            normalVelocity = Vector3.Dot(velocity, hit.Normal) * hit.Normal;
            // Reflection
            velocity = velocity - normalVelocity - bounciness * normalVelocity;

            N = -Vector3.Dot(hit.Normal, G) * hit.Normal;
            
            /*print("Pos:" + hit.Position);
            print("Norm" + hit.Normal);*/
            print("Hit");
        }

        Vector3 acceleration = new Vector3();
        acceleration = (G + N) / m;

        velocity += acceleration * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }
}
