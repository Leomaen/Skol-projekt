using UnityEngine;
using UnityEngine.UIElements;

// This class is deprecated. Camera management is now handled by CameraManager
public class RoomCollider : MonoBehaviour
{
    // Keeping minimal functionality for backward compatibility
    [SerializeField] GameObject Camera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Camera != null)
        {
            Camera.SetActive(true);
        }
    }
}
