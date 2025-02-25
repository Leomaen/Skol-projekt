using UnityEngine;

public class Door : MonoBehaviour
{

    [SerializeField] private int teleportDistance = 5;

    [SerializeField] GameObject Camera;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            if (Camera != null)
            {
                Camera.SetActive(false);
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
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
