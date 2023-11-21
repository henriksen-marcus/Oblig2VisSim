using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class RainManager : MonoBehaviour
{
    [SerializeField] private GameObject raindropPrefab;
    List<BallPhysics> pooledObjects = new();
    [SerializeField] private Bounds spawnBounds = new Bounds(Vector3.zero, new Vector3(50, 5, 50));
    /// <summary>
    /// How much time between a rain drop spawns.
    /// </summary>
    [SerializeField] private float spawnRate = 0.1f;
    /// <summary>
    /// How many rain drops to spawn per spawnRate.
    /// </summary>
    [SerializeField] private int spawnAmount = 1;
    [SerializeField] private float raindropDespawnY = -40f;
    
    
    void Start()
    {
        InvokeRepeating(nameof(SpawnRaindrop), 0, spawnRate);
    }
    
    void Update()
    {
        foreach (var drop in pooledObjects.Where(drop => drop.gameObject.activeSelf))
        {
            if (drop.transform.position.y > raindropDespawnY)
                continue;
            
            drop.gameObject.SetActive(false);
            drop.Reset();
            //print("Set to inactive");
            break;
        }
    }

    void SpawnRaindrop()
    {
        int spawnCount = 0;
        
        // Check if we have a ball in the pool
        foreach (var drop in pooledObjects.Where(drop => !drop.gameObject.activeSelf))
        {
            drop.transform.position = GetRandPosition();
            drop.gameObject.SetActive(true);
            print("Drops in the pool: " + pooledObjects.Count());
            spawnCount++;
            
            if (spawnCount == spawnAmount)
                return;
        }

        while (spawnCount < spawnAmount)
        {
            var instantiated = Instantiate(raindropPrefab, GetRandPosition(), Quaternion.identity).GetComponent<BallPhysics>();
            pooledObjects.Add(instantiated);
            print("Drops in the pool: " + pooledObjects.Count());
            spawnCount++;
        }
    }

    private Vector3 GetRandPosition()
    {
        var randPos = new Vector3(
            UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x),
            UnityEngine.Random.Range(spawnBounds.min.y, spawnBounds.max.y),
            UnityEngine.Random.Range(spawnBounds.min.z, spawnBounds.max.z)
        );
        return randPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);
    }
}
