using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    // Singleton
    public static BallManager Instance;
    
    [SerializeField] private GameObject ballPrefab;
    
    // Ball pool. We use this to avoid creating and destroying balls.
    List<GameObject> balls = new List<GameObject>();

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
        
    }

    public void SpawnBall(Vector3 position)
    {
        balls.Add(Instantiate(ballPrefab, position, Quaternion.identity) as GameObject);
    }
}
