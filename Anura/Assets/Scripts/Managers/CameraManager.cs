using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;
    public static CameraManager Instance { get { return _instance; } }

    private Dictionary<Vector2Int, GameObject> roomCameras = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int currentRoomIndex;
    public Vector2Int CurrentRoomIndex => currentRoomIndex;
    
    // A more robust way to track active cameras
    private GameObject currentActiveCamera = null;
    
    // Ensure we don't switch cameras too frequently
    [SerializeField] private float switchCooldown = 0.5f;
    private float lastSwitchTime = 0f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("CameraManager initialized");
        
        // Force disable all cameras at start
        ForcefullyDisableAllCameras();
    }

    public void RegisterCamera(Vector2Int roomIndex, GameObject camera)
    {
        if (!roomCameras.ContainsKey(roomIndex))
        {
            roomCameras.Add(roomIndex, camera);
            Debug.Log($"Registered camera for room at index {roomIndex}");
            
            // Make sure it's inactive when registered
            if (camera != null)
            {
                camera.SetActive(false);
                
                // Set camera tag for easier finding
                camera.tag = "RoomCamera";
            }
        }
    }

    public void ActivateCameraForRoom(Vector2Int roomIndex)
    {
        // Don't switch too frequently
        if (Time.time - lastSwitchTime < switchCooldown)
            return;
            
        // Skip if already in this room
        if (roomIndex.Equals(currentRoomIndex) && currentActiveCamera != null && currentActiveCamera.activeSelf)
            return;
            
        Debug.Log($"===== SWITCHING CAMERA from room {currentRoomIndex} to room {roomIndex} =====");
        lastSwitchTime = Time.time;
        
        // Start by forcefully disabling ALL cameras in the scene
        ForcefullyDisableAllCameras();
        
        // Then activate just the one we want
        if (roomCameras.ContainsKey(roomIndex) && roomCameras[roomIndex] != null)
        {
            currentActiveCamera = roomCameras[roomIndex];
            currentActiveCamera.SetActive(true);
            currentRoomIndex = roomIndex;
            Debug.Log($"Camera for room at {roomIndex} ACTIVATED: {currentActiveCamera.name}");
        }
        else
        {
            Debug.LogWarning($"No camera found for room at index {roomIndex}");
            currentActiveCamera = null;
        }
    }
    
    // More aggressive camera disabling
    private void ForcefullyDisableAllCameras()
    {
        // First, directly find and disable ALL cameras in the scene
                Camera[] allCamerasInScene = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        
        foreach (Camera cam in allCamerasInScene)
        {
            // Skip main camera if it exists
            if (cam.CompareTag("MainCamera"))
                continue;
                
            // Disable the camera's GameObject
            if (cam.gameObject != null)
            {
                Debug.Log($"Forcefully deactivating camera: {cam.gameObject.name}");
                cam.gameObject.SetActive(false);
            }
        }
        
        // Also iterate through our registered cameras to be extra sure
        foreach (var cameraEntry in roomCameras)
        {
            if (cameraEntry.Value != null)
            {
                cameraEntry.Value.SetActive(false);
            }
        }
        
        // We can also try to find by tag if we set one
        GameObject[] taggedCameras = GameObject.FindGameObjectsWithTag("RoomCamera");
        foreach (GameObject camObj in taggedCameras)
        {
            camObj.SetActive(false);
        }
    }

    public void LogAllCameras()
    {
        Debug.Log("========= CAMERA DEBUG INFO =========");
        Debug.Log($"Current room index: {currentRoomIndex}");
        Debug.Log($"Current active camera: {(currentActiveCamera != null ? currentActiveCamera.name : "none")}");
        Debug.Log($"Total registered cameras: {roomCameras.Count}");
        
        foreach (var entry in roomCameras)
        {
            string status = "unknown";
            if (entry.Value == null)
                status = "null";
            else
                status = entry.Value.activeSelf ? "ACTIVE" : "inactive";
                
            Debug.Log($"Room {entry.Key}: Camera {(entry.Value != null ? entry.Value.name : "null")} - {status}");
        }
        
        // Also log any cameras found in scene
                Camera[] sceneCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log($"Total cameras in scene: {sceneCameras.Length}");
        foreach (Camera cam in sceneCameras)
        {
            Debug.Log($"Scene camera: {cam.name}, Active: {cam.gameObject.activeSelf}");
        }
        Debug.Log("===================================");
    }
    
    // For debugging - call this from other scripts if needed
    public void DebugLogActiveCameras()
    {
        Debug.Log("--- Active Cameras Check ---");
        Camera[] activeCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in activeCameras)
        {
            Debug.Log($"Active camera found: {cam.name}");
        }
    }
}
