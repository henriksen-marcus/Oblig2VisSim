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
    /// <summary>
    /// If more balls than this exist in the pool,they get destroyed after going inactive.
    /// </summary>
    [SerializeField] private int maxDesiredDropsInPool = 100;
    
    
    void Start()
    {
        //InvokeRepeating(nameof(SpawnRaindrop), 0, spawnRate);
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
        
        //print(pooledObjects.Count(drop => drop.gameObject.activeSelf));
        
        List<BallPhysics> dropsToRemove = new();
        int numDropsToRemove = pooledObjects.Count - maxDesiredDropsInPool;
        int numDropsRemoved = 0;
        foreach (var drop in pooledObjects.Where(drop => drop.gameObject.activeSelf).Where(drop => drop.transform.position.y < raindropDespawnY))
        {

            if (numDropsRemoved < numDropsToRemove)
            {
                dropsToRemove.Add(drop);
                numDropsRemoved++;
            }
            else
            {
                drop.gameObject.SetActive(false);
                drop.Reset();
            }
            break;
        }

        foreach (var ball in dropsToRemove)
        {
            pooledObjects.Remove(ball);
            Destroy(ball.gameObject);
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
            //print("Drops in the pool: " + pooledObjects.Count());
            spawnCount++;
            
            if (spawnCount == spawnAmount)
                return;
        }

        while (spawnCount < spawnAmount)
        {
            var instantiated = Instantiate(raindropPrefab, GetRandPosition(), Quaternion.identity).GetComponent<BallPhysics>();
            pooledObjects.Add(instantiated);
            //print("Drops in the pool: " + pooledObjects.Count());
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

    public void ToggleRain()
    {
        if (IsInvoking(nameof(SpawnRaindrop)))
            CancelInvoke(nameof(SpawnRaindrop));
        else
            InvokeRepeating(nameof(SpawnRaindrop), 0, spawnRate);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);
    }
}
