using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private int teleportDistance = 5;
    [SerializeField] private Camera roomCamera; // Reference to the camera in the current room
    
    [SerializeField] private bool isLocked = false;
    [SerializeField] private GameObject lockedVisual; // Visual indicator when door is locked
    
    private BoxCollider2D doorCollider;

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        if (doorCollider == null)
        {
            doorCollider = gameObject.AddComponent<BoxCollider2D>();
            doorCollider.isTrigger = true;
        }
        
        // Initialize locked visual if it exists
        if (lockedVisual != null)
        {
            lockedVisual.SetActive(false);
        }
    }

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
        if (collision.CompareTag("Player") && !isLocked)
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
    
    public void LockDoor()
    {
        isLocked = true;
        
        // Show the locked visual if it exists
        if (lockedVisual != null)
        {
            lockedVisual.SetActive(true);
        }
        
        // Visual feedback - you could change the door's appearance here
        GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f); // Darken the door
    }
    
    public void UnlockDoor()
    {
        isLocked = false;
        
        // Hide the locked visual if it exists
        if (lockedVisual != null)
        {
            lockedVisual.SetActive(false);
        }
        
        // Reset the door's appearance
    GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    }
}