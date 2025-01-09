using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private GameObject topDoor;
    [SerializeField] private GameObject bottomDoor;
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;

    private Vector2Int gridPosition;
    private bool isBossRoom = false;
    private RoomDirection? connectingDoor = null;

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

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