using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    [SerializeField] GameObject normalRoomPrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int minRooms = 10;
    [SerializeField] private List<RoomData> specialRooms;

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
        roomGrid = new int[gridSizeX, gridSizeY];
        roomQueue = new Queue<Vector2Int>();

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
        var initialRoom = Instantiate(normalRoomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        initialRoom.name = $"Room-{roomCount}";
        initialRoom.GetComponent<Room>().RoomIndex = roomIndex;
        roomObjects.Add(initialRoom);
    }

    private void Update()
    {
        if(roomQueue.Count > 0 && roomCount < maxRooms && !generationComplete)
        {
            Vector2Int roomIndex = roomQueue.Dequeue();
            int gridX = roomIndex.x;
            int gridY = roomIndex.y;

            TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
            TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
        }
        else if (roomCount < minRooms){
            Debug.Log($"RoomCount was less than minRooms. Trying again");
            RegenerateRooms();
        }
        else if (!generationComplete)
        {
            // Check if we have all required special rooms
            EnsureAllSpecialRoomsSpawned();
            
            Debug.Log($"Generation complete, {roomCount} rooms created");
            generationComplete = true; 
            
            // Spawn player in Room-1
            GameObject startingRoom = GameObject.Find("Room-1");
            if (startingRoom != null)
            {
                Vector3 spawnPosition = startingRoom.transform.position;
                Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            }
            
            // Debug camera setup
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.LogAllCameras();
            }
        }
    }

    private void EnsureAllSpecialRoomsSpawned()
    {
        // Group special rooms by type
        var roomTypeGroups = specialRooms.GroupBy(room => room.roomType);
        
        foreach (var group in roomTypeGroups)
        {
            RoomType type = group.Key;
            bool typeExistsInLevel = group.Any(r => r.currentCount > 0);
            
            // If this type doesn't exist in the level, try to spawn it
            if (!typeExistsInLevel && roomCount < maxRooms)
            {
                Vector2Int? emptyPosition = FindEmptyRoomPosition();
                if (emptyPosition.HasValue)
                {
                    SpawnSpecialRoomAt(emptyPosition.Value, group.First());
                }
            }
        }
    }

    private Vector2Int? FindEmptyRoomPosition()
    {
        // Find a position adjacent to existing rooms
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (roomGrid[x, y] == 0 && CountAdjacentRooms(new Vector2Int(x, y)) == 1)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return null;
    }

    private void SpawnSpecialRoomAt(Vector2Int pos, RoomData roomData)
    {
        int x = pos.x;
        int y = pos.y;
        
        roomGrid[x, y] = 1;
        roomCount++;
        
        var newRoom = Instantiate(roomData.roomPrefab, GetPositionFromGridIndex(pos), Quaternion.identity);
        newRoom.GetComponent<Room>().RoomIndex = pos;
        newRoom.name = $"Room-{roomCount}-{roomData.roomType}";
        roomData.currentCount++;
        roomObjects.Add(newRoom);
        
        OpenDoors(newRoom, x, y);
        
        Debug.Log($"Forced spawning of {roomData.roomType} room at {pos}");
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

        if(UnityEngine.Random.value < 0.5f && roomIndex != Vector2Int.zero)
            return false;

        if(CountAdjacentRooms(roomIndex) > 1)
            return false;

        // Try to spawn a special room
        RoomData specialRoom = null;
        specialRoom = ChooseSpecialRoom();

        roomQueue.Enqueue(roomIndex);
        roomGrid[x, y] = 1;
        roomCount++;

        GameObject roomPrefab = (specialRoom != null) ? specialRoom.roomPrefab : normalRoomPrefab;
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
        // Increase probability as we approach max rooms
        float normalProbability = 0.2f;
        float adjustedProbability = normalProbability;
        
        if (roomCount > minRooms * 0.7f)
        {
            // Gradually increase probability as we get closer to the max rooms
            float progress = (float)(roomCount - minRooms * 0.7f) / (maxRooms - minRooms * 0.7f);
            adjustedProbability = Mathf.Lerp(normalProbability, 0.5f, progress);
        }
        
        if (UnityEngine.Random.value < adjustedProbability)
        {
            // Prioritize room types that haven't been spawned yet
            var unspawnedRooms = specialRooms.FindAll(r => r.currentCount == 0 && r.currentCount < r.maxPerFloor);
            if (unspawnedRooms.Count > 0)
            {
                return unspawnedRooms[UnityEngine.Random.Range(0, unspawnedRooms.Count)];
            }
            
            // Fall back to any available room
            var availableRooms = specialRooms.FindAll(r => r.currentCount < r.maxPerFloor);
            if (availableRooms.Count > 0)
            {
                return availableRooms[UnityEngine.Random.Range(0, availableRooms.Count)];
            }
        }
        
        return null;
    }

    private void RegenerateRooms(){
        // Reset special room counts
        foreach (var room in specialRooms)
        {
            room.currentCount = 0;
        }

        roomObjects.ForEach(Destroy);
        roomObjects.Clear();
        roomGrid = new int[gridSizeX, gridSizeY];
        roomQueue.Clear();
        roomCount = 0;
        generationComplete = false;

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    void OpenDoors(GameObject room, int x, int y) {
        Room newRoomScript = room.GetComponent<Room>();
        if (newRoomScript == null)
        {
            Debug.LogError($"Room script missing on {room.name}");
            return;
        }

        Room leftRoomScript = GetRoomScriptAt(new Vector2Int(x - 1, y));
        Room rightRoomScript = GetRoomScriptAt(new Vector2Int(x + 1, y));
        Room topRoomScript = GetRoomScriptAt(new Vector2Int(x, y + 1));
        Room bottomRoomScript = GetRoomScriptAt(new Vector2Int(x, y - 1));

        if(x > 0 && roomGrid[x - 1, y] != 0 && leftRoomScript != null)
        {
            newRoomScript.OpenDoor(Vector2Int.left);
            leftRoomScript.OpenDoor(Vector2Int.right);
            Debug.Log($"Opening doors between {room.name} and left room");
        }
        if(x < gridSizeX - 1 && roomGrid[x + 1, y] != 0 && rightRoomScript != null)
        {
            newRoomScript.OpenDoor(Vector2Int.right);
            rightRoomScript.OpenDoor(Vector2Int.left);
            Debug.Log($"Opening doors between {room.name} and right room");
        }
        if(y > 0 && roomGrid[x, y - 1] != 0 && bottomRoomScript != null)
        {
            newRoomScript.OpenDoor(Vector2Int.down);
            bottomRoomScript.OpenDoor(Vector2Int.up);
            Debug.Log($"Opening doors between {room.name} and bottom room");
        }
        if(y < gridSizeY - 1 && roomGrid[x, y + 1] != 0 && topRoomScript != null)
        {
            newRoomScript.OpenDoor(Vector2Int.up);
            topRoomScript.OpenDoor(Vector2Int.down);
            Debug.Log($"Opening doors between {room.name} and top room");
        }
    }

    Room GetRoomScriptAt(Vector2Int index){
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

        if(x > 0 && roomGrid[x - 1, y] != 0) count ++;
        if(x < gridSizeX - 1 && roomGrid[x + 1, y] != 0) count ++;
        if(y > 0 && roomGrid[x, y - 1] != 0) count ++;
        if(y < gridSizeY - 1 && roomGrid[x, y + 1] != 0) count ++; 

        return count;
    }

    private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex) {
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
