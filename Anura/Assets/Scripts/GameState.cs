using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObjects/GameState")]
public class GameState : ScriptableObject
{
  public WorldState world = new();

  public void OnEnable()
  {
    RoomManager.OnGenerationComplete += Save;
  }

  public void OnDisable()
  {
    RoomManager.OnGenerationComplete -= Save;
  }

  public void Save()
  {
    string path = Path.Combine(Application.persistentDataPath, "save.json");
    try
    {
      string json = JsonUtility.ToJson(this, true);
      File.WriteAllText(path, json);
      Debug.Log($"Game saved to: {path}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"Failed to save game: {e.Message}");
    }
  }
  
  public void Load()
  {
    string path = Path.Combine(Application.persistentDataPath, "save.json");
    try
    {
      string json = File.ReadAllText(path);
      JsonUtility.FromJsonOverwrite(json, this);
    }
    catch (System.Exception e)
    {
      Debug.LogError($"Failed to load game: {e.Message}");
    }
  }
}

[Serializable]
public class WorldState
{
  public int seed = 0;
  public bool isGenerated = false;
  // public Dictionary<Vector2Int, RoomInfo> rooms = new Dictionary<Vector2Int, RoomInfo>();
}
