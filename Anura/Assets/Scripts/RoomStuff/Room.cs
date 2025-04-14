using System.Numerics;
using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [SerializeField] GameObject topDoorTree; 
    [SerializeField] GameObject bottomDoorTree; 
    [SerializeField] GameObject rightDoorTree; 
    [SerializeField] GameObject leftDoorTree;
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

    [SerializeField] private bool isRoomCleared = false;
    private List<GameObject> enemiesInRoom = new List<GameObject>();
    private bool playerInRoom = false;
    private bool doorsLocked = false;

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

        // Find all enemies in the room at start
        FindEnemiesInRoom();
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

        // Room is considered cleared if there are no enemies
        isRoomCleared = enemiesInRoom.Count == 0;
    }
    
    private void Update()
    {
        if (playerInRoom && !doorsLocked && enemiesInRoom.Count > 0)
        {
            LockDoors();
        }
        
        // Check if all enemies are defeated
        if (playerInRoom && doorsLocked)
        {
            // Remove any null references (destroyed enemies)
            enemiesInRoom.RemoveAll(enemy => enemy == null);
            
            if (enemiesInRoom.Count == 0)
            {
                UnlockDoors();
                isRoomCleared = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRoom = true;
            
            // Only switch if we're not in a cooldown period
            if (Time.time - lastCameraSwitch >= cameraSwitchCooldown)
            {
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.ActivateCameraForRoom(RoomIndex);
                    lastCameraSwitch = Time.time;
                }
            }
            
            // Lock doors if room is not cleared and has enemies
            if (!isRoomCleared && enemiesInRoom.Count > 0)
            {
                LockDoors();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRoom = false;
        }
    }

    public void OpenDoor(Vector2Int direction)
    {
        if (direction == Vector2Int.up && topDoor != null) 
        {
            topDoor.SetActive(true); 
            topDoorTree.SetActive(false); 
            Debug.Log($"Opening top door in {gameObject.name}");
        }
        if (direction == Vector2Int.down && bottomDoor != null) 
        {
            bottomDoor.SetActive(true); 
            bottomDoorTree.SetActive(false); 
            Debug.Log($"Opening bottom door in {gameObject.name}");
        }
        if (direction == Vector2Int.left && leftDoor != null) 
        {
            leftDoor.SetActive(true); 
            leftDoorTree.SetActive(false); 
            Debug.Log($"Opening left door in {gameObject.name}");
        }
        if (direction == Vector2Int.right && rightDoor != null) 
        {
            rightDoor.SetActive(true); 
            rightDoorTree.SetActive(false); 
            Debug.Log($"Opening right door in {gameObject.name}");
        }
    }
    
    // Finds all enemies in the room
    private void FindEnemiesInRoom()
    {
        // Find all objects with the "Enemy" tag that are children of this room
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Enemy"))
            {
                enemiesInRoom.Add(child.gameObject);
            }
        }
        
        Debug.Log($"Room {gameObject.name} has {enemiesInRoom.Count} enemies");
    }
    
    // Locks all doors when player enters a room with enemies
    private void LockDoors()
    {
        doorsLocked = true;
        
        if (topDoor) topDoor.GetComponent<Door>().LockDoor();
        if (bottomDoor) bottomDoor.GetComponent<Door>().LockDoor();
        if (leftDoor) leftDoor.GetComponent<Door>().LockDoor();
        if (rightDoor) rightDoor.GetComponent<Door>().LockDoor();
        
        Debug.Log($"Doors locked in room {gameObject.name}");
    }
    
    // Unlocks all doors when all enemies are defeated
    private void UnlockDoors()
    {
        doorsLocked = false;
        
        if (topDoor) topDoor.GetComponent<Door>().UnlockDoor();
        if (bottomDoor) bottomDoor.GetComponent<Door>().UnlockDoor();
        if (leftDoor) leftDoor.GetComponent<Door>().UnlockDoor();
        if (rightDoor) rightDoor.GetComponent<Door>().UnlockDoor();
        
        Debug.Log($"Doors unlocked in room {gameObject.name} - all enemies defeated!");
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
