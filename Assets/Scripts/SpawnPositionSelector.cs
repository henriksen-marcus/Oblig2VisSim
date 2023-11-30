using System;
using UnityEngine;

/// <summary>
/// Allows the user to select a spawn position for the ball.
/// Draws a hologram of the ball at the hovered position.
/// </summary>
public class SpawnPositionSelector : MonoBehaviour
{
    [Header("Set these references")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject hologramPrefab;
    /// <summary>
    /// The vertical offset of the spawn position from the surface.
    /// Don't set this to less than the radius of the ball.
    /// </summary>
    [SerializeField] [Min(0f)] private float ySpawnOffset = 5f;
    [SerializeField] private Vector3 spawnCameraOffset = new Vector3(12f, 8f, 0f);
    [SerializeField] private Vector2 spawnCameraFovRange = new Vector2(50f, 110f);

    /// <summary>
    /// We allow the user to scroll the mouse wheel to offset the spawn position.
    /// </summary>
    private float yCustomOffset;
    private Vector3 lastValidSpawnPos;
    private GameObject hologram;
    private Camera spawnCamera;
    private BallManager ballManager;

    [SerializeField] private TriangleSurface triangleSurface;
    
    void Start()
    {
        hologram = Instantiate(hologramPrefab);
        hologram.SetActive(false);
        
        spawnCamera = gameObject.AddComponent<Camera>();
        spawnCamera.rect = new Rect(0f, 0.7f, 0.3f, 0.3f);
        spawnCamera.fieldOfView = (spawnCameraFovRange.x + spawnCameraFovRange.y) / 2;
        
        ballManager = BallManager.Instance;
        
        if (WorldManager.Instance != null)
            triangleSurface = WorldManager.Instance.triangleSurface;
    }
    
    private void OnDestroy()
    {
        Destroy(hologram);
    }
    
    void Update()
    {
        // Get scroll wheel input, don't let it go below 0 [DEPRECATED]
        //yCustomOffset = Mathf.Max(yCustomOffset + Input.mouseScrollDelta.y * 2, 0);
        spawnCamera.fieldOfView = Mathf.Clamp(spawnCamera.fieldOfView - Input.mouseScrollDelta.y * 3, spawnCameraFovRange.x, spawnCameraFovRange.y);
        
        // Reset custom offset on mouse wheel click
        if (Input.GetMouseButtonDown(2))
        {
            //yCustomOffset = 0;
            spawnCamera.fieldOfView = (spawnCameraFovRange.x + spawnCameraFovRange.y) / 2;
        }

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            // Draw hologram
            lastValidSpawnPos = hit.point;
            lastValidSpawnPos.y += ySpawnOffset/* + yCustomOffset*/;
            hologram.transform.position = lastValidSpawnPos;
            hologram.SetActive(true);
            
            // Move camera
            spawnCamera.transform.position = hologram.transform.position + spawnCameraOffset;
            spawnCamera.transform.LookAt(hologram.transform.position);
            
            spawnCamera.enabled = true;
            
            if (Input.GetMouseButtonDown(0))
            {
                ballManager.SpawnBall(lastValidSpawnPos);
            }
            
            var p = new Vector2(hit.point.x, hit.point.z);
            triangleSurface.DrawTriangleAtPosition(p);
        }
        else
        {
            hologram.SetActive(false);
            spawnCamera.enabled = false;
        }
    }
    
    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        /*Gizmos.DrawWireSphere(lastValidSpawnPos, 10f);#1#
        Gizmos.DrawRay(lastValidSpawnPos, Vector3.up * 1000f);
    }*/
    
    public Vector3 GetSpawnPosition() => lastValidSpawnPos;
}
