using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomarsdManager : MonoBehaviour
{
  [SerializeField] List<GameObject> normalRoomPrefabs;
  [SerializeField] GameObject playerPrefab;
  [SerializeField] private int maxRooms = 15;
  [SerializeField] private int minRooms = 10;
  [SerializeField] private List<RoomData> specialRooms;
  [SerializeField] GameState gameState;
  [SerializeField] private int maxRegenerationAttempts = 5;

  // Events
  public static event Action OnGenerationComplete;
  public static event Action<Vector2Int> OnRoomCleared;

  // Generation state
  private int regenerationAttempts = 0;
  private int seed;
  private bool generationComplete = false;

  // Room tracking
  private List<GameObject> roomObjects = new List<GameObject>();
  private Queue<Vector2Int> roomQueue = new Queue<Vector2Int>();
  private int[,] roomGrid;
  private int roomCount;
  private List<Vector2Int> branchEndRooms = new List<Vector2Int>();

  // Special room flags
  private bool hasBossRoomSpawned = false;
  private bool hasTreasureRoomSpawned = false;
  private bool hasShopRoomSpawned = false;

  // Room dimensions
  int roomWidth = 20;
  int roomHeight = 12;
  [SerializeField] int gridSizeX = 10;
  [SerializeField] int gridSizeY = 10;

  private void Start()
  {
    GenerateLevel();
  }

  private void GenerateLevel()
  {
    // Create or use existing seed
    if (gameState.world.seed == 0 || !gameState.world.isGenerated)
    {
      gameState.world.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
      gameState.world.isGenerated = false;
      Debug.Log($"Generated new seed: {gameState.world.seed}");
    }

    seed = gameState.world.seed;
    regenerationAttempts = 0;

    InitializeGeneration();
  }

  private void InitializeGeneration()
  {
    // Clean up any existing rooms
    CleanupExistingRooms();

    // Reset state
    UnityEngine.Random.InitState(seed);
    Debug.Log($"Using seed: {seed}, Attempt: {regenerationAttempts + 1}");

    roomGrid = new int[gridSizeX, gridSizeY];
    roomQueue = new Queue<Vector2Int>();
    roomCount = 0;

    // Reset special room tracking
    hasBossRoomSpawned = false;
    hasTreasureRoomSpawned = false;
    hasShopRoomSpawned = false;
    branchEndRooms.Clear();

    // Reset special room counts
    foreach (var room in specialRooms)
    {
      room.currentCount = 0;
    }

    // Start from center room
    Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
    StartRoomGenerationFromRoom(initialRoomIndex);
  }

  private void CleanupExistingRooms()
  {
    foreach (var room in roomObjects)
    {
      if (room != null)
      {
        Destroy(room);
      }
    }
    roomObjects.Clear();
    roomQueue.Clear();
    generationComplete = false;
  }

  private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
  {
    roomQueue.Enqueue(roomIndex);
    roomGrid[roomIndex.x, roomIndex.y] = 1;
    roomCount++;

    // Create starting room
    GameObject normalRoomPrefab = normalRoomPrefabs[UnityEngine.Random.Range(0, normalRoomPrefabs.Count)];
    var initialRoom = Instantiate(normalRoomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
    initialRoom.name = $"Room-{roomCount}";
    initialRoom.GetComponent<Room>().RoomIndex = roomIndex;
    roomObjects.Add(initialRoom);
  }

  private void Update()
  {
    if (!generationComplete)
    {
      // Continue room generation
      if (roomQueue.Count > 0 && roomCount < maxRooms)
      {
        Vector2Int roomIndex = roomQueue.Dequeue();
        GenerateAdjacentRooms(roomIndex);
      }
      else if (roomCount < minRooms)
      {
        // Not enough rooms generated
        Debug.Log($"RoomCount was less than minRooms. Trying again");
        AttemptRegeneration();
      }
      else
      {
        // Finalize level generation
        FinalizeLevelGeneration();
      }
    }
  }

  private void GenerateAdjacentRooms(Vector2Int roomIndex)
  {
    // Try to generate rooms in all 4 directions
    TryGenerateRoom(new Vector2Int(roomIndex.x - 1, roomIndex.y)); // Left
    TryGenerateRoom(new Vector2Int(roomIndex.x + 1, roomIndex.y)); // Right
    TryGenerateRoom(new Vector2Int(roomIndex.x, roomIndex.y + 1)); // Up
    TryGenerateRoom(new Vector2Int(roomIndex.x, roomIndex.y - 1)); // Down
  }

  private void FinalizeLevelGeneration()
  {
    // Find valid branch ends
    UpdateBranchEndRooms();

    // Attempt to place special rooms
    AttemptToPlaceSpecialRooms();

    if (HasAllRequiredRooms())
    {
      // Successfully generated all required rooms
      Debug.Log($"Generation complete, {roomCount} rooms created");
      gameState.world.isGenerated = true;
      gameState.world.seed = seed;
      generationComplete = true;

      // Trigger the event
      OnGenerationComplete?.Invoke();
    }
    else
    {
      // Failed to place all required rooms
      Debug.Log("Failed to place all required rooms. Regenerating...");
      AttemptRegeneration();
    }
  }

  private void AttemptRegeneration()
  {
    regenerationAttempts++;

    // If we've tried too many times with this seed, modify it
    if (regenerationAttempts >= maxRegenerationAttempts)
    {
      seed = seed + regenerationAttempts; // Change seed slightly
      Debug.Log($"Maximum regeneration attempts reached. Modifying seed to: {seed}");
    }

    InitializeGeneration();
  }

  private void AttemptToPlaceSpecialRooms()
  {
    // Boss room placement - always at branch end
    if (!hasBossRoomSpawned && branchEndRooms.Count > 0)
    {
      SpawnBossRoom();
    }

    // Force boss room placement if we've tried multiple times
    if (!hasBossRoomSpawned && regenerationAttempts >= 2 && roomObjects.Count > 0)
    {
      ForceBossRoomPlacement();
    }

    // Place treasure and shop rooms
    if (!hasTreasureRoomSpawned)
    {
      SpawnSpecialRoom(RoomType.Treasure);
    }

    if (!hasShopRoomSpawned)
    {
      SpawnSpecialRoom(RoomType.Shop);
    }
  }

  private void UpdateBranchEndRooms()
  {
    branchEndRooms.Clear();

    // Find all branch end rooms (rooms with only one connection)
    for (int x = 0; x < gridSizeX; x++)
    {
      for (int y = 0; y < gridSizeY; y++)
      {
        if (roomGrid[x, y] == 1)
        {
          Vector2Int index = new Vector2Int(x, y);
          if (CountAdjacentRooms(index) == 1) // Only one connection
          {
            // Make sure it's not the starting room
            GameObject room = FindRoomAt(index);
            if (room != null && room.name != "Room-1")
            {
              branchEndRooms.Add(index);
            }
          }
        }
      }
    }

    Debug.Log($"Found {branchEndRooms.Count} branch end rooms suitable for boss placement");
  }

  // Helper method to find a room at a specific grid position
  private GameObject FindRoomAt(Vector2Int position)
  {
    return roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == position);
  }

  private bool TryGenerateRoom(Vector2Int roomIndex)
  {
    int x = roomIndex.x;
    int y = roomIndex.y;

    // Validate position
    if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
      return false;

    // Check if room already exists
    if (roomGrid[x, y] != 0)
      return false;

    // Check max rooms limit
    if (roomCount >= maxRooms)
      return false;

    // Skip chance decreases with more regeneration attempts
    float skipChance = Mathf.Max(0, 0.5f - (0.1f * regenerationAttempts));
    if (UnityEngine.Random.value < skipChance)
      return false;

    // Control room adjacency to create better branch patterns
    int maxAdjacent = (regenerationAttempts >= 3) ? 2 : 1;
    if (CountAdjacentRooms(roomIndex) > maxAdjacent)
      return false;

    // Add room to the grid
    roomQueue.Enqueue(roomIndex);
    roomGrid[x, y] = 1;
    roomCount++;

    // Determine room type (normal by default)
    GameObject roomPrefab = normalRoomPrefabs[UnityEngine.Random.Range(0, normalRoomPrefabs.Count)];
    string roomName = $"Room-{roomCount}";

    // Create the room
    var newRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
    newRoom.GetComponent<Room>().RoomIndex = roomIndex;
    newRoom.name = roomName;
    roomObjects.Add(newRoom);

    // Handle door connections
    OpenDoors(newRoom, x, y);

    return true;
  }

  private void SpawnBossRoom()
  {
    if (branchEndRooms.Count == 0) return;

    // Choose a random branch end for the boss room
    Vector2Int bossRoomIndex = branchEndRooms[UnityEngine.Random.Range(0, branchEndRooms.Count)];

    ReplaceWithSpecialRoom(bossRoomIndex, RoomType.Boss);
  }

  private void ForceBossRoomPlacement()
  {
    // Pick any room except the starting room
    List<GameObject> possibleRooms = roomObjects.FindAll(r => r.name != "Room-1");

    if (possibleRooms.Count > 0)
    {
      GameObject targetRoom = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];
      Vector2Int roomIndex = targetRoom.GetComponent<Room>().RoomIndex;

      ReplaceWithSpecialRoom(roomIndex, RoomType.Boss);
      Debug.Log("Boss room forcefully placed after multiple attempts");
    }
  }

  private void SpawnSpecialRoom(RoomType roomType)
  {
    // Find all eligible room positions (excluding branch ends and starting room)
    List<Vector2Int> availableLocations = new List<Vector2Int>();

    for (int x = 0; x < gridSizeX; x++)
    {
      for (int y = 0; y < gridSizeY; y++)
      {
        Vector2Int index = new Vector2Int(x, y);

        // Skip if no room here, branch end, or starting room
        if (roomGrid[x, y] == 0 || branchEndRooms.Contains(index))
          continue;

        GameObject room = FindRoomAt(index);
        if (room != null && room.name == "Room-1")
          continue;

        availableLocations.Add(index);
      }
    }

    if (availableLocations.Count > 0)
    {
      Vector2Int roomIndex = availableLocations[UnityEngine.Random.Range(0, availableLocations.Count)];
      ReplaceWithSpecialRoom(roomIndex, roomType);
    }
  }

  private void ReplaceWithSpecialRoom(Vector2Int roomIndex, RoomType roomType)
  {
    // Find the special room data
    RoomData specialRoomData = specialRooms.Find(r => r.roomType == roomType);
    if (specialRoomData == null) return;

    // Find and remove the existing room
    GameObject existingRoom = FindRoomAt(roomIndex);
    if (existingRoom != null)
    {
      roomObjects.Remove(existingRoom);
      Destroy(existingRoom);

      // Create the new special room
      var specialRoom = Instantiate(specialRoomData.roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
      specialRoom.GetComponent<Room>().RoomIndex = roomIndex;
      specialRoom.name = $"Room-{roomCount}-{roomType}";
      roomObjects.Add(specialRoom);

      // Connect doors
      OpenDoors(specialRoom, roomIndex.x, roomIndex.y);

      // Update tracking
      specialRoomData.currentCount++;

      if (roomType == RoomType.Boss)
        hasBossRoomSpawned = true;
      else if (roomType == RoomType.Treasure)
        hasTreasureRoomSpawned = true;
      else if (roomType == RoomType.Shop)
        hasShopRoomSpawned = true;

      Debug.Log($"{roomType} room placed at {roomIndex}");
    }
  }

  private bool HasAllRequiredRooms()
  {
    return hasBossRoomSpawned && hasTreasureRoomSpawned && hasShopRoomSpawned;
  }

  // Existing methods (simplified if needed)
  private void OpenDoors(GameObject room, int x, int y)
  {
    Room newRoomScript = room.GetComponent<Room>();

    Room leftRoomScript = GetRoomScriptAt(new Vector2Int(x - 1, y));
    Room rightRoomScript = GetRoomScriptAt(new Vector2Int(x + 1, y));
    Room topRoomScript = GetRoomScriptAt(new Vector2Int(x, y + 1));
    Room bottomRoomScript = GetRoomScriptAt(new Vector2Int(x, y - 1));

    if (x > 0 && roomGrid[x - 1, y] != 0)
    {
      newRoomScript.OpenDoor(Vector2Int.left);
      leftRoomScript?.OpenDoor(Vector2Int.right);
    }
    if (x < gridSizeX - 1 && roomGrid[x + 1, y] != 0)
    {
      newRoomScript.OpenDoor(Vector2Int.right);
      rightRoomScript?.OpenDoor(Vector2Int.left);
    }
    if (y > 0 && roomGrid[x, y - 1] != 0)
    {
      newRoomScript.OpenDoor(Vector2Int.down);
      bottomRoomScript?.OpenDoor(Vector2Int.up);
    }
    if (y < gridSizeY - 1 && roomGrid[x, y + 1] != 0)
    {
      newRoomScript.OpenDoor(Vector2Int.up);
      topRoomScript?.OpenDoor(Vector2Int.down);
    }
  }

  Room GetRoomScriptAt(Vector2Int index)
  {
    GameObject roomObject = FindRoomAt(index);
    return roomObject?.GetComponent<Room>();
  }

  private int CountAdjacentRooms(Vector2Int roomIndex)
  {
    int x = roomIndex.x;
    int y = roomIndex.y;
    int count = 0;

    if (x > 0 && roomGrid[x - 1, y] != 0) count++;
    if (x < gridSizeX - 1 && roomGrid[x + 1, y] != 0) count++;
    if (y > 0 && roomGrid[x, y - 1] != 0) count++;
    if (y < gridSizeY - 1 && roomGrid[x, y + 1] != 0) count++;

    return count;
  }

  private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
  {
    return new Vector3(
        roomWidth * (gridIndex.x - gridSizeX / 2),
        roomHeight * (gridIndex.y - gridSizeY / 2)
    );
  }

  // Room cleared notification method
  public void NotifyRoomCleared(Vector2Int roomIndex)
  {
    OnRoomCleared?.Invoke(roomIndex);
  }
}