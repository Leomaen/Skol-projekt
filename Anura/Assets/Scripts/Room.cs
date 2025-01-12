using UnityEngine;

/// <summary>
/// Manages individual room behavior including door states and position tracking
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private GameObject topDoor;
    [SerializeField] private GameObject bottomDoor;
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;

    [SerializeField] private Camera roomCamera;

    // Room state tracking
    private Vector2Int gridPosition;
    private bool isBossRoom = false;
    private RoomDirection? connectingDoor = null;

    private void Awake()
    {
        if (roomCamera == null)
        {
            roomCamera = GetComponentInChildren<Camera>(true);
        }
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
        if (roomCamera == null)
        {
            roomCamera = GetComponentInChildren<Camera>(true);
        }
        
        if (roomCamera != null)
        {
            CameraManager.Instance.RegisterRoomCamera(position, roomCamera);
        }
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    /// <summary>
    /// Returns the world position of a specified door
    /// </summary>
    public Vector3 GetDoorPosition(RoomDirection direction)
    {
        switch (direction)
        {
            case RoomDirection.Top: return topDoor.transform.position;
            case RoomDirection.Bottom: return bottomDoor.transform.position;
            case RoomDirection.Left: return leftDoor.transform.position;
            case RoomDirection.Right: return rightDoor.transform.position;
            default: return transform.position;
        }
    }

    /// <summary>
    /// Configures the room as a boss room, deactivating all doors initially
    /// </summary>
    public void SetAsBossRoom()
    {
        isBossRoom = true;
        // Deactivate all doors initially
        topDoor.SetActive(false);
        bottomDoor.SetActive(false);
        leftDoor.SetActive(false);
        rightDoor.SetActive(false);
    }

    public bool IsBossRoom()
    {
        return isBossRoom;
    }

    /// <summary>
    /// Controls the active state of doors, with special handling for boss rooms
    /// </summary>
    public void SetDoorActive(RoomDirection direction, bool active)
    {
        if (isBossRoom)
        {
            // For boss room, only set the first active door and remember it
            if (active && connectingDoor == null)
            {
                connectingDoor = direction;
            }

            switch (direction)
            {
                case RoomDirection.Top:
                    topDoor.SetActive(direction == connectingDoor);
                    break;
                case RoomDirection.Bottom:
                    bottomDoor.SetActive(direction == connectingDoor);
                    break;
                case RoomDirection.Left:
                    leftDoor.SetActive(direction == connectingDoor);
                    break;
                case RoomDirection.Right:
                    rightDoor.SetActive(direction == connectingDoor);
                    break;
            }
        }
        else
        {
            // Normal room behavior
            switch (direction)
            {
                case RoomDirection.Top:
                    topDoor.SetActive(active);
                    break;
                case RoomDirection.Bottom:
                    bottomDoor.SetActive(active);
                    break;
                case RoomDirection.Left:
                    leftDoor.SetActive(active);
                    break;
                case RoomDirection.Right:
                    rightDoor.SetActive(active);
                    break;
            }
        }
    }

    
}

