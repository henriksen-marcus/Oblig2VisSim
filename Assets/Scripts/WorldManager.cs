using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void Task2_2()
    {
        SceneManager.LoadScene("Task 2.2");
    }
    
    public void Task2_3()
    {
        SceneManager.LoadScene("Task 2.3-2.4");
    }

    public void Task3()
    {
        SceneManager.LoadScene("Task 3");
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
