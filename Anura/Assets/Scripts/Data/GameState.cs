using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Scriptable Objects/GameState")]
public class GameState : ScriptableObject
{

  private readonly string saveName = "game-state.json";
  private string savePath;
  public WorldState world = new();

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
    world = new WorldState();
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
}

[Serializable]
public class WorldState
{
  public int seed = 0;
  public int floor = 1;
  public bool isGenerated = false;
}
