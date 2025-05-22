using UnityEngine;
using System.Collections; // Required for IEnumerator

public class Portal : MonoBehaviour
{
    private RoomManager roomManager;
    private SceneFader sceneFader; // Add reference to SceneFader

    void Start()
    {
        roomManager = FindAnyObjectByType<RoomManager>();
        if (roomManager == null)
        {
            Debug.LogError("Portal could not find RoomManager instance!");
        }

        // Find the SceneFader instance in the scene
        sceneFader = FindAnyObjectByType<SceneFader>();
        if (sceneFader == null)
        {
            Debug.LogError("Portal could not find SceneFader instance!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (roomManager != null && sceneFader != null)
            {
                // Prevent further interaction with the portal
                GetComponent<Collider2D>().enabled = false; 
                StartCoroutine(TransitionToNextFloor());
            }
            else
            {
                if (roomManager == null) Debug.LogError("RoomManager not found, cannot go to next floor.");
                if (sceneFader == null) Debug.LogError("SceneFader not found, cannot start transition.");
            }
        }
    }

    private IEnumerator TransitionToNextFloor()
    {
        Debug.Log("Player entered portal. Starting transition to next floor.");
        AudioManager.Instance.PlaySound("Teleport"); // Example sound

        // Start fade out (assuming you have a "Goop" or similar fade type)
        sceneFader.FadeOut(SceneFader.FadeType.Goop); // Or your specific goop fade type

        // Wait for the fade out to complete
        yield return new WaitForSeconds(sceneFader.FadeDuration);

        // Destroy the portal before proceeding to next floor
        Destroy(gameObject);

        // Proceed to the next floor logic
        roomManager.GoToNextFloorAndRepositionPlayer();
    }
}