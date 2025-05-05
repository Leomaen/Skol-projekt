using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] List<GameObject> normalRoomPrefabs;  // List of normal room prefabs
    [SerializeField] GameObject playerPrefab;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int minRooms = 10;
    [SerializeField] private List<RoomData> specialRooms;
    public GameState gameState;
    [SerializeField] private int maxRegenerationAttempts = 5;

    public static event Action OnGenerationComplete;

    // Track regeneration attempts
    private int regenerationAttempts = 0;
    private int seed;
    private int originalSeed;

    // Special room requirement tracking
    private bool hasBossRoomSpawned = false;
    private bool hasTreasureRoomSpawned = false;
    private bool hasShopRoomSpawned = false;

    // For tracking branch end rooms (potential boss room locations)
    private List<Vector2Int> branchEndRooms = new List<Vector2Int>();

    int roomWidth = 20;
    int roomHeight = 12;
    [SerializeField] int gridSizeX = 10;
    [SerializeField] int gridSizeY = 10;

    private List<GameObject> roomObjects = new List<GameObject>();
    private Queue<Vector2Int> roomQueue = new Queue<Vector2Int>();

    private int[,] roomGrid;

    private int roomCount;

    private bool generationComplete = false;

    private void Start()
    {
        InitializeSeed();
        regenerationAttempts = 0;
        InitializeGeneration();
    }

    private void InitializeSeed()
    {
        if (gameState.world.seed == 0)  // Using 0 as default/unset value
        {
            gameState.world.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"Generated new seed: {gameState.world.seed}");
        }

        originalSeed = gameState.world.seed;
        seed = originalSeed;
    }

    private void InitializeGeneration()
    {
        UnityEngine.Random.InitState(seed);
        Debug.Log($"Using seed: {seed}, Attempt: {regenerationAttempts + 1}");

        roomGrid = new int[gridSizeX, gridSizeY];
        roomQueue = new Queue<Vector2Int>();
        roomCount = 0;

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
    {
        roomQueue.Enqueue(roomIndex);
        int x = roomIndex.x;
        int y = roomIndex.y;
        roomGrid[x, y] = 1;
        roomCount++;

        // Choose a UnityEngine.Random normal room from the list
        GameObject normalRoomPrefab = normalRoomPrefabs[UnityEngine.Random.Range(0, normalRoomPrefabs.Count)];
        var initialRoom = Instantiate(normalRoomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        initialRoom.name = $"Room-{roomCount}";
        initialRoom.GetComponent<Room>().RoomIndex = roomIndex;
        roomObjects.Add(initialRoom);
    }

    private void Update()
    {
        if (roomQueue.Count > 0 && roomCount < maxRooms && !generationComplete)
        {
            Vector2Int roomIndex = roomQueue.Dequeue();
            int gridX = roomIndex.x;
            int gridY = roomIndex.y;

            TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
            TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
        }
        else if (roomCount < minRooms)
        {
            Debug.Log($"RoomCount was less than minRooms. Trying again");
            RegenerateRooms();
        }
        else if (!generationComplete)
        {
            // Check if we need to force spawn the special rooms before completing
            UpdateBranchEndRooms(); // Update branch ends first

            // Attempt to place special rooms
            AttemptToPlaceSpecialRooms();

            // Finalize generation if all required rooms are present
            if (HasAllRequiredRooms())
            {
                Debug.Log($"Generation complete, {roomCount} rooms created");
                gameState.world.seed = seed;
                gameState.world.isGenerated = true;
                generationComplete = true;

                OnGenerationComplete?.Invoke();
            }
            else
            {
                // If we still don't have all required rooms, regenerate
                Debug.Log("Failed to place all required rooms. Regenerating...");
                RegenerateRooms();
            }
        }
    }

    private void AttemptToPlaceSpecialRooms()
    {
        // First place the boss room at a branch end
        if (!hasBossRoomSpawned)
        {
            // If we've tried multiple times already, force boss room placement
            if (regenerationAttempts >= 2 && roomObjects.Count > 0)
            {
                ForceBossRoomPlacement();
            }
            else if (branchEndRooms.Count > 0)
            {
                SpawnBossRoom();
            }
        }

        // Then place treasure and shop rooms if needed
        if (!hasTreasureRoomSpawned)
        {
            SpawnSpecialRoom(RoomType.Treasure);
        }

        if (!hasShopRoomSpawned)
        {
            SpawnSpecialRoom(RoomType.Shop);
        }
    }

    private void ForceBossRoomPlacement()
    {
        // If no branch ends, pick any room except the starting room
        List<GameObject> possibleRooms = roomObjects.FindAll(r => r.name != "Room-1");

        if (possibleRooms.Count > 0)
        {
            GameObject targetRoom = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];
            Vector2Int roomIndex = targetRoom.GetComponent<Room>().RoomIndex;

            RoomData bossRoomData = specialRooms.Find(r => r.roomType == RoomType.Boss);
            if (bossRoomData != null)
            {
                roomObjects.Remove(targetRoom);
                Destroy(targetRoom);

                var bossRoom = Instantiate(bossRoomData.roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
                bossRoom.GetComponent<Room>().RoomIndex = roomIndex;
                bossRoom.name = $"Room-{roomCount}-Boss";
                roomObjects.Add(bossRoom);

                OpenDoors(bossRoom, roomIndex.x, roomIndex.y);

                bossRoomData.currentCount++;
                hasBossRoomSpawned = true;

                Debug.Log("Boss room forcefully placed after multiple attempts");
            }
        }
    }

    private bool HasAllRequiredRooms()
    {
        return hasBossRoomSpawned && hasTreasureRoomSpawned && hasShopRoomSpawned;
    }

    private void UpdateBranchEndRooms()
    {
        branchEndRooms.Clear();

        // Check each room to see if it's a branch end (only one connection)
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (roomGrid[x, y] == 1)
                {
                    Vector2Int index = new Vector2Int(x, y);
                    if (CountAdjacentRooms(index) == 1)
                    {
                        // Check if this room isn't the starting room
                        GameObject room = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == index);
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

    private void SpawnBossRoom()
    {
        if (branchEndRooms.Count == 0) return;

        // Pick a UnityEngine.Random branch end for the boss room
        Vector2Int bossRoomIndex = branchEndRooms[UnityEngine.Random.Range(0, branchEndRooms.Count)];

        // Find the boss room data
        RoomData bossRoomData = specialRooms.Find(r => r.roomType == RoomType.Boss);
        if (bossRoomData == null) return;

        // Find and remove the existing room at this location
        GameObject existingRoom = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == bossRoomIndex);
        if (existingRoom != null)
        {
            roomObjects.Remove(existingRoom);
            Destroy(existingRoom);

            // Spawn the boss room
            var bossRoom = Instantiate(bossRoomData.roomPrefab, GetPositionFromGridIndex(bossRoomIndex), Quaternion.identity);
            bossRoom.GetComponent<Room>().RoomIndex = bossRoomIndex;
            bossRoom.name = $"Room-{roomCount}-Boss";
            roomObjects.Add(bossRoom);

            // Open doors to connect to adjacent rooms
            int x = bossRoomIndex.x;
            int y = bossRoomIndex.y;
            OpenDoors(bossRoom, x, y);

            bossRoomData.currentCount++;
            hasBossRoomSpawned = true;

            Debug.Log("Boss room placed at a branch end");
        }
    }

    private void SpawnSpecialRoom(RoomType roomType)
    {
        RoomData specialRoomData = specialRooms.Find(r => r.roomType == roomType);
        if (specialRoomData == null) return;

        // Try to find an available location for the special room
        List<Vector2Int> availableLocations = new List<Vector2Int>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2Int index = new Vector2Int(x, y);

                // Skip if no room here or if it's a branch end (reserved for boss rooms)
                if (roomGrid[x, y] == 0 || branchEndRooms.Contains(index))
                    continue;

                // Skip the starting room
                GameObject room = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == index);
                if (room != null && room.name == "Room-1")
                    continue;

                availableLocations.Add(index);
            }
        }

        if (availableLocations.Count == 0) return;

        // Pick a UnityEngine.Random location
        Vector2Int roomIndex = availableLocations[UnityEngine.Random.Range(0, availableLocations.Count)];

        // Find and remove the existing room
        GameObject existingRoom = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == roomIndex);
        if (existingRoom != null)
        {
            roomObjects.Remove(existingRoom);
            Destroy(existingRoom);

            // Spawn the special room
            var specialRoom = Instantiate(specialRoomData.roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
            specialRoom.GetComponent<Room>().RoomIndex = roomIndex;
            specialRoom.name = $"Room-{roomCount}-{roomType}";
            roomObjects.Add(specialRoom);

            // Open doors
            int x = roomIndex.x;
            int y = roomIndex.y;
            OpenDoors(specialRoom, x, y);

            specialRoomData.currentCount++;

            if (roomType == RoomType.Treasure)
                hasTreasureRoomSpawned = true;
            else if (roomType == RoomType.Shop)
                hasShopRoomSpawned = true;

            Debug.Log($"{roomType} room placed");
        }
    }

    private bool TryGenerateRoom(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;

        // Check if position is within grid bounds
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return false;

        // Check if room already exists at this position
        if (roomGrid[x, y] != 0)
            return false;

        if (roomCount >= maxRooms)
            return false;

        // Calculate skip chance based on regeneration attempts
        // As attempts increase, skip chance decreases
        float skipChance = Mathf.Max(0, 0.5f - (0.1f * regenerationAttempts));

        // Skip chance reduces with regeneration attempts, making more rooms generate
        if (UnityEngine.Random.value < skipChance && roomIndex != Vector2Int.zero)
            return false;

        // After enough regeneration attempts, be more lenient with adjacent rooms
        int maxAdjacent = (regenerationAttempts >= 3) ? 2 : 1;
        if (CountAdjacentRooms(roomIndex) > maxAdjacent)
            return false;

        // Try to spawn a special room
        RoomData specialRoom = null;

        // Chance for special rooms increases with regeneration attempts
        float specialRoomChance = 0.2f + (0.1f * regenerationAttempts);
        if (UnityEngine.Random.value < specialRoomChance)
        {
            specialRoom = ChooseSpecialRoom();

            // Update tracking flags for required special rooms
            if (specialRoom != null)
            {
                if (specialRoom.roomType == RoomType.Boss)
                {
                    // For boss rooms, we become more lenient with placement after multiple attempts
                    if (regenerationAttempts < 2 && CountAdjacentRooms(roomIndex) != 1)
                    {
                        specialRoom = null; // Not a branch end, can't place boss room here
                    }
                    else
                    {
                        hasBossRoomSpawned = true;
                    }
                }
                else if (specialRoom.roomType == RoomType.Treasure)
                {
                    hasTreasureRoomSpawned = true;
                }
                else if (specialRoom.roomType == RoomType.Shop)
                {
                    hasShopRoomSpawned = true;
                }
            }
        }

        roomQueue.Enqueue(roomIndex);
        roomGrid[x, y] = 1;
        roomCount++;

        GameObject roomPrefab;
        if (specialRoom != null)
        {
            roomPrefab = specialRoom.roomPrefab;
        }
        else
        {
            // Pick a random normal room from the list
            roomPrefab = normalRoomPrefabs[UnityEngine.Random.Range(0, normalRoomPrefabs.Count)];
        }

        var newRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        newRoom.GetComponent<Room>().RoomIndex = roomIndex;
        newRoom.name = $"Room-{roomCount}";
        if (specialRoom != null)
        {
            specialRoom.currentCount++;
            newRoom.name += $"-{specialRoom.roomType}";
        }
        roomObjects.Add(newRoom);

        OpenDoors(newRoom, x, y);

        return true;
    }

    private RoomData ChooseSpecialRoom()
    {
        var availableRooms = specialRooms.FindAll(r => r.currentCount < r.maxPerFloor);
        if (availableRooms.Count > 0)
        {
            return availableRooms[UnityEngine.Random.Range(0, availableRooms.Count)];
        }
        return null;
    }

    private void RegenerateRooms()
    {
        // Reset special room counts
        foreach (var room in specialRooms)
        {
            room.currentCount = 0;
        }

        // Reset special room tracking
        hasBossRoomSpawned = false;
        hasTreasureRoomSpawned = false;
        hasShopRoomSpawned = false;
        branchEndRooms.Clear();

        roomObjects.ForEach(Destroy);
        roomObjects.Clear();
        roomQueue.Clear();
        generationComplete = false;

        regenerationAttempts++;

        // If we've tried too many times with this seed, modify it slightly
        if (regenerationAttempts >= maxRegenerationAttempts)
        {
            seed = originalSeed + regenerationAttempts;
            Debug.Log($"Maximum regeneration attempts reached. Modifying seed to: {seed}");
        }

        InitializeGeneration();
    }

    void OpenDoors(GameObject room, int x, int y)
    {
        Room newRoomScript = room.GetComponent<Room>();

        Room leftRoomScript = GetRoomScriptAt(new Vector2Int(x - 1, y));
        Room rightRoomScript = GetRoomScriptAt(new Vector2Int(x + 1, y));
        Room topRoomScript = GetRoomScriptAt(new Vector2Int(x, y + 1));
        Room bottomRoomScript = GetRoomScriptAt(new Vector2Int(x, y - 1));

        if (x > 0 && roomGrid[x - 1, y] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.left);
            leftRoomScript.OpenDoor(Vector2Int.right);
        }
        if (x < gridSizeX - 1 && roomGrid[x + 1, y] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.right);
            rightRoomScript.OpenDoor(Vector2Int.left);
        }
        if (y > 0 && roomGrid[x, y - 1] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.down);
            bottomRoomScript.OpenDoor(Vector2Int.up);
        }
        if (y < gridSizeY - 1 && roomGrid[x, y + 1] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.up);
            topRoomScript.OpenDoor(Vector2Int.down);
        }
    }

    Room GetRoomScriptAt(Vector2Int index)
    {
        GameObject roomObject = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == index);
        if (roomObject != null)
            return roomObject.GetComponent<Room>();
        return null;
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
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;
        return new Vector3(roomWidth * (gridX - gridSizeX / 2), roomHeight * (gridY - gridSizeY / 2));
    }

    private void OnDrawGizmos()
    {
        Color gizmoColor = new Color(0, 1, 1, 0.05f);
        Gizmos.color = gizmoColor;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 position = GetPositionFromGridIndex(new Vector2Int(x, y));
                Gizmos.DrawWireCube(position, new Vector3(roomWidth, roomHeight, 1));
            }
        }
    }
}