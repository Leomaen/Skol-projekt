using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private int teleportDistance = 5;
    [SerializeField] private Camera roomCamera; // Reference to the camera in the current room
    
    [SerializeField] private bool isLocked = false;
    [SerializeField] private GameObject lockedVisual; // Visual indicator when door is locked
    
    // Add reference to SceneFader
    [SerializeField] private SceneFader sceneFader;
    
    private BoxCollider2D doorCollider;
    private bool isTransitioning = false;

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
        
        // If no sceneFader assigned, try to find it
        if (sceneFader == null)
        {
            sceneFader = FindObjectOfType<SceneFader>();
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
        if (collision.CompareTag("Player") && !isLocked && !isTransitioning)
        {
            // Start door transition coroutine
            StartCoroutine(DoorTransition(collision.transform));
        }
    }
    
    private IEnumerator DoorTransition(Transform player)
    {
        isTransitioning = true;
        
        // Fade out
        sceneFader.FadeOut(SceneFader.FadeType.PlainBlack);
        
        // Wait for fade to complete
        yield return new WaitForSeconds(sceneFader.FadeDuration);
        
        // Deactivate this room's camera
        if (roomCamera != null)
        {
            roomCamera.gameObject.SetActive(false);
        }

        // Calculate teleport position
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
        
        // Teleport the player
        player.position += teleportOffset;
        
        // Small delay before fading back in
        yield return new WaitForSeconds(0.1f);
        
        // Fade back in
        sceneFader.FadeIn(SceneFader.FadeType.PlainBlack);
        
        // Wait for fade to complete
        yield return new WaitForSeconds(sceneFader.FadeDuration);
        
        isTransitioning = false;
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