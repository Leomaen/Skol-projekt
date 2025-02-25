using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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
            Debug.Log($"Generation complete, {roomCount} rooms created");
            generationComplete = true; 
            
            // Spawn player in Room-1
            GameObject startingRoom = GameObject.Find("Room-1");
            if (startingRoom != null)
            {
                Vector3 spawnPosition = startingRoom.transform.position;
                Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            }
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

        if(UnityEngine.Random.value < 0.5f && roomIndex != Vector2Int.zero)
            return false;

        if(CountAdjacentRooms(roomIndex) > 1)
            return false;

        // Try to spawn a special room
        RoomData specialRoom = null;
        if (UnityEngine.Random.value < 0.2f) // 20% chance for special room
        {
            specialRoom = ChooseSpecialRoom();
        }

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
        var availableRooms = specialRooms.FindAll(r => r.currentCount < r.maxPerFloor);
        if (availableRooms.Count > 0)
        {
            return availableRooms[UnityEngine.Random.Range(0, availableRooms.Count)];
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

        Room leftRoomScript = GetRoomScriptAt(new Vector2Int(x - 1, y));
        Room rightRoomScript = GetRoomScriptAt(new Vector2Int(x + 1, y));
        Room topRoomScript = GetRoomScriptAt(new Vector2Int(x, y + 1));
        Room bottomRoomScript = GetRoomScriptAt(new Vector2Int(x, y - 1));

        if(x > 0 && roomGrid[x - 1, y] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.left);
            leftRoomScript.OpenDoor(Vector2Int.right);
        }
        if(x < gridSizeX - 1 && roomGrid[x + 1, y] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.right);
            rightRoomScript.OpenDoor(Vector2Int.left);
        }
        if(y > 0 && roomGrid[x, y - 1] != 0 )
        {
            newRoomScript.OpenDoor(Vector2Int.down);
            bottomRoomScript.OpenDoor(Vector2Int.up);
        }
        if(y < gridSizeY - 1 && roomGrid[x, y + 1] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.up);
            topRoomScript.OpenDoor(Vector2Int.down);
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
