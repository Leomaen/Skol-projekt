using System.Numerics;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    
    // Add these new variables for enemy spawning
    [SerializeField] private List<EnemySpawnPoint> enemySpawnPoints = new List<EnemySpawnPoint>();
    private bool enemiesSpawned = false;
    [SerializeField] private int enemySpawnDelay = 0;
    private bool enemiesSpawning = false;

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

        // Room is initially considered cleared if no spawn points
        isRoomCleared = enemySpawnPoints.Count == 0;
    }
    
    private void Update()
    {
        if (playerInRoom && !doorsLocked && enemiesInRoom.Count > 0)
        {
            LockDoors();
        }
        
        // Check if all enemies are defeated
    if (playerInRoom && doorsLocked && !enemiesSpawning)
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
            
            // Spawn enemies if this room hasn't been cleared yet
            if (!isRoomCleared && !enemiesSpawned)
            {
                LockDoors();

                SpawnEnemies();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRoom = false;
            
            // Destroy enemies when leaving the room if the room isn't cleared
            if (!isRoomCleared && !doorsLocked) 
            {
                DestroyEnemies();
            }
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
    
    
    // Spawn enemies when entering the room
    private void SpawnEnemies()
    {
        enemiesInRoom.Clear();
        enemiesSpawning = true; // Set the flag before starting the spawn
        StartCoroutine(SpawnEnemiesWithDelay());
        enemiesSpawned = true;
    }

    private IEnumerator SpawnEnemiesWithDelay()
    {
        yield return new WaitForSeconds(enemySpawnDelay);

        foreach (var spawnPoint in enemySpawnPoints)
        {
            if (spawnPoint != null && spawnPoint.enemyPrefab != null)
            {
                GameObject enemy = Instantiate(spawnPoint.enemyPrefab, spawnPoint.transform.position, UnityEngine.Quaternion.identity, transform);
                
                // Add special handling for rattleEnemy to ensure proper initialization
                rattleEnemy rattleComp = enemy.GetComponent<rattleEnemy>();
                if (rattleComp != null)
                {
                    // Optional initialization for rattleEnemy
                }
                
                enemiesInRoom.Add(enemy);
            }
        }
        
        Debug.Log($"Spawned {enemiesInRoom.Count} enemies in room {gameObject.name} after delay");
        enemiesSpawning = false;

    }
    
    // Destroy all enemies when leaving the room
    private void DestroyEnemies()
    {
        foreach (var enemy in enemiesInRoom)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        enemiesInRoom.Clear();
        enemiesSpawned = false;
        Debug.Log($"Destroyed all enemies in room {gameObject.name}");
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
        
        if (topDoor && topDoor.activeSelf) topDoor.GetComponent<Door>().UnlockDoor();
        if (bottomDoor && bottomDoor.activeSelf) bottomDoor.GetComponent<Door>().UnlockDoor();
        if (leftDoor && leftDoor.activeSelf) leftDoor.GetComponent<Door>().UnlockDoor();
        if (rightDoor && rightDoor.activeSelf) rightDoor.GetComponent<Door>().UnlockDoor();
        
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
