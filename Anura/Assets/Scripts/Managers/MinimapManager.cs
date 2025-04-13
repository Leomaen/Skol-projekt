// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class MinimapManager : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private RoomManager roomManager;
//     [SerializeField] private Transform minimapContainer;
//     [SerializeField] private GameObject minimapRoomPrefab;
    
//     [Header("Room Icons")]
//     [SerializeField] private Sprite normalRoomIcon;
//     [SerializeField] private Sprite bossRoomIcon;
//     [SerializeField] private Sprite treasureRoomIcon;
//     [SerializeField] private Sprite shopRoomIcon;
//     [SerializeField] private Sprite startRoomIcon;
//     [SerializeField] private Sprite currentRoomIcon;
    
//     [Header("Settings")]
//     [SerializeField] private float roomIconSize = 10f;
//     [SerializeField] private float mapScale = 0.5f;
    
//     private Dictionary<Vector2Int, GameObject> minimapIcons = new Dictionary<Vector2Int, GameObject>();
//     private Vector2Int currentRoomIndex;
//     private Vector2Int previousRoomIndex;
    
//     private void Start()
//     {
//         if (roomManager == null)
//             roomManager = FindObjectOfType<RoomManager>();
//     }
    
//     public void RevealRoomOnMinimap(Vector2Int roomIndex, RoomType roomType)
//     {
//         // Return if the room is already on the minimap
//         if (minimapIcons.ContainsKey(roomIndex))
//             return;
            
//         // Create minimap icon at the correct position
//         GameObject minimapIcon = Instantiate(minimapRoomPrefab, minimapContainer);
//         Image iconImage = minimapIcon.GetComponent<Image>();
        
//         // Scale position to fit minimap
//         RectTransform rectTransform = minimapIcon.GetComponent<RectTransform>();
//         rectTransform.anchoredPosition = new Vector2(roomIndex.x * roomIconSize, roomIndex.y * roomIconSize);
//         rectTransform.sizeDelta = new Vector2(roomIconSize, roomIconSize);
        
//         // Set correct icon based on room type
//         switch(roomType)
//         {
//             case RoomType.Boss:
//                 iconImage.sprite = bossRoomIcon;
//                 break;
//             case RoomType.Treasure:
//                 iconImage.sprite = treasureRoomIcon;
//                 break;
//             case RoomType.Shop:
//                 iconImage.sprite = shopRoomIcon;
//                 break;
//             case RoomType.Start:
//                 iconImage.sprite = startRoomIcon;
//                 break;
//             default:
//                 iconImage.sprite = normalRoomIcon;
//                 break;
//         }
        
//         // Add to dictionary
//         minimapIcons.Add(roomIndex, minimapIcon);
//     }
    
//     public void UpdateCurrentRoom(Vector2Int newRoomIndex)
//     {
//         previousRoomIndex = currentRoomIndex;
//         currentRoomIndex = newRoomIndex;
        
//         // Update the previous room icon
//         if (minimapIcons.ContainsKey(previousRoomIndex) && previousRoomIndex != Vector2Int.zero)
//         {
//             // Determine the type of the previous room and set its icon accordingly
//             RoomType roomType = GetRoomTypeAt(previousRoomIndex);
//             UpdateRoomIcon(previousRoomIndex, roomType);
//         }
        
//         // Update the current room icon
//         if (minimapIcons.ContainsKey(currentRoomIndex))
//         {
//             minimapIcons[currentRoomIndex].GetComponent<Image>().sprite = currentRoomIcon;
//         }
//         else
//         {
//             // This is the first time we entered this room, reveal it
//             RoomType roomType = GetRoomTypeAt(currentRoomIndex);
//             RevealRoomOnMinimap(currentRoomIndex, roomType);
//             minimapIcons[currentRoomIndex].GetComponent<Image>().sprite = currentRoomIcon;
//         }
//     }
    
//     private RoomType GetRoomTypeAt(Vector2Int roomIndex)
//     {
//         Room room = roomManager.GetRoomScriptAt(roomIndex);
//         return room ? room.RoomType : RoomType.Normal;
//     }
    
//     private void UpdateRoomIcon(Vector2Int roomIndex, RoomType roomType)
//     {
//         if (!minimapIcons.ContainsKey(roomIndex))
//             return;
            
//         Image iconImage = minimapIcons[roomIndex].GetComponent<Image>();
        
//         switch(roomType)
//         {
//             case RoomType.Boss:
//                 iconImage.sprite = bossRoomIcon;
//                 break;
//             case RoomType.Treasure:
//                 iconImage.sprite = treasureRoomIcon;
//                 break;
//             case RoomType.Shop:
//                 iconImage.sprite = shopRoomIcon;
//                 break;
//             case RoomType.Start:
//                 iconImage.sprite = startRoomIcon;
//                 break;
//             default:
//                 iconImage.sprite = normalRoomIcon;
//                 break;
//         }
//     }
// }