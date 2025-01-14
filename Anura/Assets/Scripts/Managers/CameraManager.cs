using UnityEngine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    private Dictionary<Vector2Int, Camera> roomCameras = new Dictionary<Vector2Int, Camera>();
    private Camera currentActiveCamera;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterRoomCamera(Vector2Int position, Camera camera)
    {
        if (camera == null) return;
        
        roomCameras[position] = camera;
        camera.gameObject.SetActive(false);
        
        if (position == Vector2Int.zero)
        {
            currentActiveCamera = camera;
            camera.gameObject.SetActive(true);
        }
    }

    public void SwitchToRoom(Vector2Int position)
    {
        if (currentActiveCamera != null)
            currentActiveCamera.gameObject.SetActive(false);

        if (roomCameras.TryGetValue(position, out Camera newCamera))
        {
            currentActiveCamera = newCamera;
            newCamera.gameObject.SetActive(true);
        }
    }
}