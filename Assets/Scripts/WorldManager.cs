using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;
    
    public TriangleSurface triangleSurface { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    void Start()
    {
        triangleSurface = GameObject.FindObjectOfType<TriangleSurface>();
    }
    
    void Update()
    {
        
    }
}
