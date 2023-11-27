using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    // Singleton
    public static BallManager Instance;
    
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float ballDespawnY = -50f;
    /// <summary>
    /// If more balls than this exist in the pool,they get destroyed after going inactive.
    /// </summary>
    [SerializeField] private int maxDesiredBallsInPool = 20;
    
    /// <summary>
    /// Ball pool. We use this to avoid creating and destroying balls.
    /// </summary>
    List<BallPhysics> balls = new();

    private List<BallPhysics> pooledObjects1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        List<BallPhysics> ballsToRemove = new();
        int numBallsToRemove = balls.Count - maxDesiredBallsInPool;
        int numBallsRemoved = 0;
        foreach (var ball in balls.Where(ball => ball.gameObject.activeSelf).Where(ball => ball.transform.position.y < ballDespawnY))
        {
            if (numBallsRemoved < numBallsToRemove)
            {
                ballsToRemove.Add(ball);
                numBallsRemoved++;
            }
            else
            {
                ball.gameObject.SetActive(false);
                ball.Reset();
            }
            break;
        }
        //print("deleting " + ballsToRemove.Count + " balls");
        // Remove balls from the pool
        foreach (var ball in ballsToRemove)
        {
            balls.Remove(ball);
            Destroy(ball.gameObject);
        }
    }

    public void SpawnBall(Vector3 position)
    {
        // Check if we have a ball in the pool
        foreach (var ball in balls)
        {
            if (ball.gameObject.activeSelf) continue;
            ball.transform.position = position;
            ball.gameObject.SetActive(true);
            //print("Balls in the pool: " + balls.Count());
            return;
        }
        var instantiated = Instantiate(ballPrefab, position, Quaternion.identity).GetComponent<BallPhysics>();
        balls.Add(instantiated);
        //print("Balls in the pool: " + balls.Count());
    }
}
