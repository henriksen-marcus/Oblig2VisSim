using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [SerializeField] TriangleSurface triangleSurface;
    Vector3 g = Physics.gravity;
    float m = 1f;
    Vector3 velocity = Vector3.zero;
    Vector3 prevNormal = Vector3.zero;
    [SerializeField] [Range(0,1)] float bounciness = 0; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        var position = transform.position;
        Vector3 pos = position;
        Vector2 pos2D = new Vector2(pos.x, pos.z);
        var hit = triangleSurface.GetCollision(pos2D);

        Vector3 newVel = velocity;
        Vector3 N = new Vector3();
        Vector3 G = m * g;
        Vector3 force = new Vector3();

        var thisy = Mathf.Abs(hit.Position.y);
        var thaty = Mathf.Abs(position.y);
        
        bool validY = Math.Max(thisy, thaty) - Mathf.Min(thisy, thaty) <= transform.localScale.y / 2;

        if (hit.isHit && validY)
        {
            print("Hit");
            force += Vector3.Dot(velocity, hit.Normal) * hit.Normal;
            // Reflection
            force += velocity - 2 * bounciness * force;

            N = -Vector3.Dot(hit.Normal, G) * hit.Normal;
        }

        Vector3 acceleration = new Vector3();
        acceleration = (G + N) / m;

        velocity += acceleration * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }

    // make trangle surface class that holds the meshæ
    // this class need a function to get barycentric coordiantes and also returns a hit struct,
    // that contains the hit normal of the surface at that point
    // then compare the height to the height of the ball and check if the diff is less than or equal to the radius of the ball to see if we have contact.
}
