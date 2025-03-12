using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private int teleportDistance = 5;
    [SerializeField] private Camera roomCamera; // Reference to the camera in the current room

    private void Start()
    {
        // If roomCamera isn't assigned in the inspector, try to find the camera in the parent room
        if (roomCamera == null)
        {
            // Try to find the camera in the parent object (assuming doors are children of rooms)
            roomCamera = GetComponentInParent<Camera>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Deactivate this room's camera when exiting through the door
            if (roomCamera != null)
            {
                roomCamera.gameObject.SetActive(false);
            }

            Vector3 teleportOffset = Vector3.zero;
            
            switch (gameObject.name)
            {
                case "TopDoor":
                    teleportOffset = new Vector3(0, teleportDistance, 0);
                    break;
                case "BottomDoor":
                    teleportOffset = new Vector3(0, -teleportDistance, 0);
                    break;
                case "LeftDoor":
                    teleportOffset = new Vector3(-teleportDistance, 0, 0);
                    break;
                case "RightDoor":
                    teleportOffset = new Vector3(teleportDistance, 0, 0);
                    break;
            }
            
            collision.transform.position += teleportOffset;
            
            // The room's trigger will activate the correct camera
        }
    }
}