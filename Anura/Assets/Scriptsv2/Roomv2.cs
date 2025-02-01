using System.Numerics;
using UnityEngine;

public class Roomv2 : MonoBehaviour
{
    [SerializeField] GameObject topDoor; 
    [SerializeField] GameObject bottomDoor; 
    [SerializeField] GameObject rightDoor; 
    [SerializeField] GameObject leftDoor; 

    public Vector2Int RoomIndex { get; set; }
    public void OpenDoor(Vector2Int direction)
    {
        if (direction == Vector2Int.up) 
        {
            topDoor.SetActive(true); 
        }
        if (direction == Vector2Int.down) 
        {
            bottomDoor.SetActive(true); 
        }
        if (direction == Vector2Int.left) 
        {
            leftDoor.SetActive(true); 
        }
        if (direction == Vector2Int.right) 
        {
            rightDoor.SetActive(true); 
        }
    }
}
