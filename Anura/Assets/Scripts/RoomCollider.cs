using UnityEngine;

public class RoomCollider : MonoBehaviour
{

    [SerializeField] GameObject Camera;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            if (Camera != null)
            {
                Camera.SetActive(true);
            }
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
