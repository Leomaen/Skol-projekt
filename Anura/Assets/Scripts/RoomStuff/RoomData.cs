using UnityEngine;

[System.Serializable]
public class RoomData
{
    public GameObject roomPrefab;
    public RoomType roomType;
    public int maxPerFloor = 1;
    public int currentCount = 0;
}
