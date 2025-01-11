using UnityEngine;

/// <summary>
/// Handles player teleportation between rooms through doors
/// </summary>
public class DoorTrigger : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private RoomDirection direction;
    [SerializeField] private float teleportCooldown = 0.5f; // Prevents rapid teleportation
    [SerializeField] private float exitOffset = 1f;         // Distance from door when exiting
    
    private Room parentRoom;
    private static float lastTeleportTime;

    private void Start()
    {
        parentRoom = GetComponentInParent<Room>();
    }

    /// <summary>
    /// Handles player collision with door trigger and teleportation logic
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time > lastTeleportTime + teleportCooldown)
        {
            Vector2Int currentPos = parentRoom.GetGridPosition();
            Vector2Int targetPos = GetTargetRoomPosition(currentPos);
            
            Room targetRoom = RoomManager.Instance.GetRoom(targetPos);
            if (targetRoom != null)
            {
                RoomDirection exitDirection = direction; // Use the same direction we're entering from
                Vector3 targetDoorPos = targetRoom.GetDoorPosition(GetOppositeDirection(direction));
                Vector3 offset = GetOffsetDirection(exitDirection) * exitOffset; // Use entry direction for offset
                other.transform.position = targetDoorPos + offset;
                lastTeleportTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Calculates the target room's grid position based on current direction
    /// </summary>
    private Vector2Int GetTargetRoomPosition(Vector2Int currentPos)
    {
        switch (direction)
        {
            case RoomDirection.Top: return currentPos + Vector2Int.up;
            case RoomDirection.Bottom: return currentPos + Vector2Int.down;
            case RoomDirection.Left: return currentPos + Vector2Int.left;
            case RoomDirection.Right: return currentPos + Vector2Int.right;
            default: return currentPos;
        }
    }

    private RoomDirection GetOppositeDirection(RoomDirection dir)
    {
        switch (dir)
        {
            case RoomDirection.Top: return RoomDirection.Bottom;
            case RoomDirection.Bottom: return RoomDirection.Top;
            case RoomDirection.Left: return RoomDirection.Right;
            case RoomDirection.Right: return RoomDirection.Left;
            default: return dir;
        }
    }

    private Vector3 GetOffsetDirection(RoomDirection dir)
    {
        switch (dir)
        {
            case RoomDirection.Top: return Vector3.up;
            case RoomDirection.Bottom: return Vector3.down;
            case RoomDirection.Left: return Vector3.left;
            case RoomDirection.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }
}