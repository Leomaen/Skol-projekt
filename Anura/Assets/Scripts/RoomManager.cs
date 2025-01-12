using UnityEngine;
using System.Collections.Generic;

/// Defines the possible directions a room can connect to other rooms
public enum RoomDirection
{
    Top,
    Bottom,
    Left,
    Right
}

/// Manages the procedural generation and organization of rooms in the game
public class RoomManager : MonoBehaviour
{
    // Singleton instance
    public static RoomManager Instance { get; private set; }

    [Header("Room Prefabs")]
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private GameObject bossRoomPrefab;

    [Header("Generation Settings")]
    [SerializeField] private int maxRooms = 10;
    [SerializeField] private float roomSpacingX = 20f;  // Distance between rooms horizontally
    [SerializeField] private float roomSpacingY = 12f;  // Distance between rooms vertically
    [SerializeField] private bool showDebugGrid = true; // Toggle for debug visualization
    [SerializeField] private int gridSize = 5;          // Size of the debug grid
    [SerializeField] private int minBranchLength = 2;   // Minimum length of a room branch
    [SerializeField] private float branchingProbability = 0.7f; // Chance to continue a branch

    // Room tracking collections
    private Dictionary<Vector2Int, Room> roomGrid = new Dictionary<Vector2Int, Room>();
    private List<Vector2Int> availablePositions = new List<Vector2Int>();

    // Branch generation tracking
    private Vector2Int lastDirection = Vector2Int.zero;
    private int currentBranchLength = 0;
    private Vector2Int bossRoomPosition;
    private bool bossRoomPlaced = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start() => GenerateRooms();

    /// <summary>
    /// Main room generation algorithm that creates the dungeon layout
    /// </summary>
    void GenerateRooms()
    {
        bossRoomPlaced = false;
        bossRoomPosition = Vector2Int.zero;
        
        CreateRoom(Vector2Int.zero, true);
        CameraManager.Instance.SwitchToRoom(Vector2Int.zero);
        
        int roomCount = 1;
        while (roomCount < maxRooms && availablePositions.Count > 0)
        {
            List<(Vector2Int pos, float weight)> weightedPositions = new List<(Vector2Int, float)>();
            
            foreach (Vector2Int pos in availablePositions)
            {
                float distanceWeight = Mathf.Max(1f, Vector2Int.Distance(Vector2Int.zero, pos));
                float directionWeight = ShouldContinueBranch(pos) ? 2f : 1f;
                weightedPositions.Add((pos, distanceWeight * directionWeight));
            }

            float totalWeight = 0f;
            foreach (var weighted in weightedPositions)
                totalWeight += weighted.weight;

            float randomValue = Random.Range(0f, totalWeight);
            Vector2Int selectedPosition = Vector2Int.zero;
            
            foreach (var weighted in weightedPositions)
            {
                randomValue -= weighted.weight;
                if (randomValue <= 0)
                {
                    selectedPosition = weighted.pos;
                    break;
                }
            }

            if (CreateRoom(selectedPosition))
            {
                roomCount++;
                UpdateBranchInfo(selectedPosition);
                
                // Check if this could be a boss room position
                if (!bossRoomPlaced && currentBranchLength >= minBranchLength && IsEndPoint(selectedPosition))
                {
                    // Immediately place boss room and update connections
                    bossRoomPosition = selectedPosition;
                    ReplaceWithBossRoom(bossRoomPosition);
                    bossRoomPlaced = true;
                    
                    // Remove all potential positions around the boss room
                    availablePositions.RemoveAll(pos => 
                        Vector2Int.Distance(pos, bossRoomPosition) <= 1);
                }
            }
            
            availablePositions.Remove(selectedPosition);
        }

        

        // If we haven't placed a boss room yet, find the furthest endpoint
        if (!bossRoomPlaced)
        {
            float maxDistance = 0;
            Vector2Int furthestPoint = Vector2Int.zero;
            
            foreach (var roomPair in roomGrid)
            {
                float distance = Vector2Int.Distance(Vector2Int.zero, roomPair.Key);
                if (distance > maxDistance && IsEndPoint(roomPair.Key))
                {
                    maxDistance = distance;
                    furthestPoint = roomPair.Key;
                    bossRoomPlaced = true;
                    bossRoomPosition = furthestPoint;
                }
            }
            if (bossRoomPlaced)
            {
                ReplaceWithBossRoom(bossRoomPosition);
                // Remove any remaining available positions
                availablePositions.Clear();
            }
        }

        // Final door update pass
        foreach (var room in roomGrid.Values)
        {
            UpdateRoomDoors(room);
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGrid) return;
        
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int y = -gridSize; y <= gridSize; y++)
            {
                Vector3 pos = new Vector3(x * roomSpacingX, y * roomSpacingY, 0);
                Vector3 size = new Vector3(roomSpacingX, roomSpacingY, 0);
                Gizmos.DrawWireCube(pos, size);
            }
        }

        if (roomGrid != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var room in roomGrid.Values)
            {
                if (room != null)
                {
                    Vector3 pos = room.transform.position;
                    float markerSize = Mathf.Min(roomSpacingX, roomSpacingY) * 0.1f;
                    Gizmos.DrawWireSphere(pos, markerSize);
                }
            }
        }
    }

    /// <summary>
    /// Creates a room at the specified grid position
    /// </summary>
    /// <param name="position">Grid position for the new room</param>
    /// <param name="isStartRoom">Whether this is the starting room</param>
    /// <returns>True if room was successfully created</returns>
    bool CreateRoom(Vector2Int position, bool isStartRoom = false)
    {
        if (roomGrid.ContainsKey(position)) return false;

        // Don't create rooms adjacent to boss room
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            if (roomGrid.ContainsKey(position + dir) && roomGrid[position + dir].IsBossRoom())
            {
                return false;
            }
        }

        Vector3 worldPosition = new Vector3(position.x * roomSpacingX, position.y * roomSpacingY, 0);
        GameObject prefabToUse = isStartRoom ? startRoomPrefab : roomPrefab;
        
        GameObject roomObj = Instantiate(prefabToUse, worldPosition, Quaternion.identity, transform);
        Room room = roomObj.GetComponent<Room>();
        
        if (room == null) return false;

        roomGrid[position] = room;
        room.SetGridPosition(position);

        if (!isStartRoom)
        {
            CheckAndAddPosition(position + Vector2Int.up);
            CheckAndAddPosition(position + Vector2Int.down);
            CheckAndAddPosition(position + Vector2Int.left);
            CheckAndAddPosition(position + Vector2Int.right);
        }
        else
        {
            CheckAndAddPosition(position + Vector2Int.right);
        }

        return true;
    }

    private System.Collections.IEnumerator WaitForCameraAndActivate(Vector2Int position)
    {
        yield return new WaitForSeconds(0.2f);
        CameraManager.Instance.SwitchToRoom(position);
    }

    void CheckAndAddPosition(Vector2Int position)
    {
        if (!roomGrid.ContainsKey(position) && !availablePositions.Contains(position))
        {
            availablePositions.Add(position);
        }
    }

    /// <summary>
    /// Updates the active state of doors in a room based on neighboring rooms
    /// </summary>
    void UpdateRoomDoors(Room room)
    {
        Vector2Int pos = room.GetGridPosition();
        
        // Don't create doors connecting to boss room unless it's the original connecting room
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        RoomDirection[] roomDirs = { RoomDirection.Top, RoomDirection.Bottom, RoomDirection.Left, RoomDirection.Right };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int neighborPos = pos + directions[i];
            bool shouldActivate = roomGrid.ContainsKey(neighborPos);

            // If neighbor exists and is boss room, check if this is the original connecting room
            if (shouldActivate && roomGrid[neighborPos].IsBossRoom())
            {
                // Only activate if this room was placed before the boss room
                shouldActivate = pos == GetConnectingRoomPosition(neighborPos);
            }

            room.SetDoorActive(roomDirs[i], shouldActivate);
        }
    }

    private Vector2Int GetConnectingRoomPosition(Vector2Int bossPosition)
    {
        // Find the original connecting room position (the one that was there before boss room)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = bossPosition + dir;
            if (roomGrid.ContainsKey(neighborPos) && !roomGrid[neighborPos].IsBossRoom())
            {
                return neighborPos;
            }
        }
        return Vector2Int.zero;
    }

    /// <summary>
    /// Replaces an existing room with a boss room
    /// </summary>
    private void ReplaceWithBossRoom(Vector2Int position)
    {
        if (!roomGrid.ContainsKey(position)) return;

        // Find the connecting room direction before destroying the old room
        Vector2Int connectingRoomPos = GetConnectingRoomPosition(position);
        
        Destroy(roomGrid[position].gameObject);
        Vector3 worldPosition = new Vector3(position.x * roomSpacingX, position.y * roomSpacingY, 0);
        GameObject bossRoomObj = Instantiate(bossRoomPrefab, worldPosition, Quaternion.identity, transform);
        Room bossRoom = bossRoomObj.GetComponent<Room>();
        
        bossRoom.SetGridPosition(position);
        bossRoom.SetAsBossRoom();
        roomGrid[position] = bossRoom;

        // Immediately update the boss room and its connecting room's doors
        UpdateRoomDoors(bossRoom);
        if (roomGrid.ContainsKey(connectingRoomPos))
        {
            UpdateRoomDoors(roomGrid[connectingRoomPos]);
        }
    }
    
    /// <summary>
    /// Determines if the generation should continue in the current direction
    /// </summary>
    private bool ShouldContinueBranch(Vector2Int newPos)
    {
        if (currentBranchLength < minBranchLength) 
            return true;
            
        Vector2Int direction = GetDirectionFromCenter(newPos);
        return direction == lastDirection && Random.value < branchingProbability;
    }

    private void UpdateBranchInfo(Vector2Int newPos)
    {
        Vector2Int newDirection = GetDirectionFromCenter(newPos);
        
        if (newDirection == lastDirection)
            currentBranchLength++;
        else
        {
            lastDirection = newDirection;
            currentBranchLength = 1;
        }
    }

    private Vector2Int GetDirectionFromCenter(Vector2Int pos)
    {
        Vector2Int fromCenter = pos - Vector2Int.zero;
        return new Vector2Int(
            Mathf.Abs(fromCenter.x) > Mathf.Abs(fromCenter.y) ? (int)Mathf.Sign(fromCenter.x) : 0,
            Mathf.Abs(fromCenter.y) >= Mathf.Abs(fromCenter.x) ? (int)Mathf.Sign(fromCenter.y) : 0
        );
    }

    /// <summary>
    /// Checks if a position is an endpoint (has only one connection)
    /// </summary>
    private bool IsEndPoint(Vector2Int position)
    {
        if (position == Vector2Int.zero) return false; // Don't consider start room
        
        int neighborCount = 0;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (Vector2Int dir in directions)
        {
            if (roomGrid.ContainsKey(position + dir))
                neighborCount++;
        }
        
        // Only true endpoints with exactly one neighbor
        return neighborCount == 1;
    }

    public Room GetRoom(Vector2Int position)
    {
        return roomGrid.ContainsKey(position) ? roomGrid[position] : null;
    }
}