using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
       
    }

    // make trangle surface class that holds the meshæ
    // this class need a function to get barycentric coordiantes and also returns a hit struct,
    // that contains the hit normal of the surface at that point
    // then compare the height to the height of the ball and check if the diff is less than or equal to the radius of the ball to see if we have contact.
}
