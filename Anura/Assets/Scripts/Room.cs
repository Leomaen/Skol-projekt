using System.Numerics;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] RoomType roomType = RoomType.Normal;
    [SerializeField] GameObject topDoor; 
    [SerializeField] GameObject bottomDoor; 
    [SerializeField] GameObject rightDoor; 
    [SerializeField] GameObject leftDoor;
    [SerializeField] GameObject roomCamera;
    
    [SerializeField] private float cameraSwitchCooldown = 0.2f;
    private float lastCameraSwitch;
    
    private BoxCollider2D roomCollider;
    private UnityEngine.Vector2 roomSize = new UnityEngine.Vector2(19f, 11f);

    public Vector2Int RoomIndex { get; set; }

    private void Awake()
    {
        // Ensure the room has a proper collider
        roomCollider = GetComponent<BoxCollider2D>();
        if (roomCollider == null)
        {
            roomCollider = gameObject.AddComponent<BoxCollider2D>();
            roomCollider.isTrigger = true;
            roomCollider.size = roomSize;
            roomCollider.offset = UnityEngine.Vector2.zero;
        }

        // Make all doors closed (inactive) by default
        if (topDoor) topDoor.SetActive(false);
        if (bottomDoor) bottomDoor.SetActive(false);
        if (leftDoor) leftDoor.SetActive(false);
        if (rightDoor) rightDoor.SetActive(false);
    }

    private void Start()
    {
        if (roomCamera != null && CameraManager.Instance != null)
        {
            CameraManager.Instance.RegisterCamera(RoomIndex, roomCamera);
            
            // Set the first room's camera active
            if (gameObject.name == "Room-1" || gameObject.name.StartsWith("Room-1-"))
            {
                CameraManager.Instance.ActivateCameraForRoom(RoomIndex);
            }
        }
        else if (roomCamera == null)
        {
            Debug.LogError($"Room {gameObject.name} has no camera assigned!");
        }
        
        lastCameraSwitch = -cameraSwitchCooldown;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Only switch if we're not in a cooldown period
            if (Time.time - lastCameraSwitch >= cameraSwitchCooldown)
            {
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.ActivateCameraForRoom(RoomIndex);
                    lastCameraSwitch = Time.time;
                }
            }
        }
    }

    public void OpenDoor(Vector2Int direction)
    {
        if (direction == Vector2Int.up && topDoor != null) 
        {
            topDoor.SetActive(true); 
            Debug.Log($"Opening top door in {gameObject.name}");
        }
        if (direction == Vector2Int.down && bottomDoor != null) 
        {
            bottomDoor.SetActive(true); 
            Debug.Log($"Opening bottom door in {gameObject.name}");
        }
        if (direction == Vector2Int.left && leftDoor != null) 
        {
            leftDoor.SetActive(true); 
            Debug.Log($"Opening left door in {gameObject.name}");
        }
        if (direction == Vector2Int.right && rightDoor != null) 
        {
            rightDoor.SetActive(true); 
            Debug.Log($"Opening right door in {gameObject.name}");
        }
    }
    
#if UNITY_EDITOR
    // Only draw gizmos in Edit mode, not during play
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = new Color(0, 1, 0, 0.1f); // Very transparent
            Gizmos.DrawWireCube(transform.position, new UnityEngine.Vector3(roomSize.x, roomSize.y, 1));
        }
    }
#endif
}
