using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Scriptable Objects/GameState")]
public class GameState : ScriptableObject
{
  private readonly string saveName = "game-state.json";
  private string savePath;
  public WorldState world = new();
  public StatsState stats = new();

  public List<Item> activeItems = new();
  public List<WeaponModifier> activeWeaponModifiers = new();
  public List<string> collectedItemIds = new();

  // Add room state persistence
  public Dictionary<string, RoomStateData> roomStates = new();


  public void OnEnable()
  {
    savePath = Path.Combine(Application.persistentDataPath, saveName);
    Door.OnDoorCollision += Save;
    RoomManager.OnGenerationComplete += Save;
  }

  public void OnDisable()
  {
    Door.OnDoorCollision += Save;
    RoomManager.OnGenerationComplete -= Save;
  }

  public void OnDestroy()
  {
    if (HasSave())
    {
      Save();
    }
  }

  public void NewGame()
  {
    world = new();
    stats = new();
    activeItems = new();
    activeWeaponModifiers = new();
    collectedItemIds = new();
    roomStates = new();
  }

  public bool HasSave()
  {
    return File.Exists(savePath);
  }

  public void Save()
  {
    try
    {
      string json = JsonUtility.ToJson(this, true);
      File.WriteAllText(savePath, json);
      Debug.Log($"Game saved to: {savePath}");
    }
    catch (Exception e)
    {
      Debug.LogError($"Failed to save game: {e.Message}");
    }
  }

  public void Load()
  {
    if (!HasSave())
    {
      Debug.LogWarning("No save file found.");
      return;
    }
    try
    {
      string json = File.ReadAllText(savePath);
      JsonUtility.FromJsonOverwrite(json, this);
    }
    catch (Exception e)
    {
      Debug.LogError($"Failed to load game: {e.Message}");
    }
  }

  public void DeleteSave()
  {
    if (HasSave())
    {
      try
      {
        File.Delete(savePath);
        Debug.Log($"Save file deleted: {savePath}");
      }
      catch (Exception e)
      {
        Debug.LogError($"Failed to delete save file: {e.Message}");
      }
    }
    else
    {
      Debug.LogWarning("No save file found to delete.");
    }
  }

  // Methods for room state management
  public void SetRoomCleared(int floor, Vector2Int roomIndex, bool isCleared)
  {
    string key = GetRoomKey(floor, roomIndex);
    if (!roomStates.ContainsKey(key))
    {
      roomStates[key] = new RoomStateData();
    }
    roomStates[key].isCleared = isCleared;
  }

  public bool IsRoomCleared(int floor, Vector2Int roomIndex)
  {
    string key = GetRoomKey(floor, roomIndex);
    return roomStates.ContainsKey(key) && roomStates[key].isCleared;
  }

  private string GetRoomKey(int floor, Vector2Int roomIndex)
  {
    return $"F{floor}_R{roomIndex.x}_{roomIndex.y}";
  }
}

[Serializable]
public class WorldState
{
  public int seed = 0;
  public int floor = 1;
  public bool isGenerated = false;
}

[Serializable]
public class StatsState
{
  [Header("Combat Stats")]
  public int damage = 5;
  public int bulletSpeed = 10;
  public float atkSpeed = 1f;

  [Header("Health Stats")]
  public int PlayerHealth = 6;
  public int maxHealth = 6;

  [Header("Movement Stats")]
  public float movementSpeed = 7f;
}

[Serializable]
public class RoomStateData
{
  public bool isCleared = false;
  // Add more room-specific data here if needed in the future
}
