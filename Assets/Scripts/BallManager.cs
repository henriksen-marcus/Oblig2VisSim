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
    
    // Ball pool. We use this to avoid creating and destroying balls.
    List<BallPhysics> balls = new();

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
        foreach (var ball in balls)
        {
            if (ball.transform.position.y > ballDespawnY || !ball.gameObject.activeSelf)
                continue;
            
            ball.gameObject.SetActive(false);
            ball.Reset();
            print("Set to inactive");
            break;
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
            print("Balls in the pool: " + balls.Count());
            return;
        }
        var instantiated = Instantiate(ballPrefab, position, Quaternion.identity).GetComponent<BallPhysics>();
        balls.Add(instantiated);
        print("Balls in the pool: " + balls.Count());
    }
}
