using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using JetBrains.Annotations;

[CreateAssetMenu(fileName = "UserData", menuName = "Scriptable Objects/UserData")]
public class UserData : ScriptableObject
{
  public OnlineUser user;
  public StatsData stats = new();

  public string sessionToken = string.Empty;

  private readonly string saveName = "user-data.json";
  private string savePath;


  private readonly string baseUrl = "https://anura.ameow.gay/api";

  // Add a delegate for login callbacks
  public delegate void LoginCallback(bool success, string message);
  public delegate void VerifyCallback(bool success, OnlineUser user, string message);


  public void OnEnable()
  {
    savePath = Path.Combine(Application.persistentDataPath, saveName);
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

  public void SignOut()
  {
    user = null;
    sessionToken = string.Empty;
    Save();
  }

  public async void Verify(VerifyCallback callback = null)
  {
    if (string.IsNullOrEmpty(sessionToken))
    {
      Debug.LogWarning("Session token is empty. User is not logged in.");
      callback?.Invoke(false, null, "User is not logged in");
      return;
    }

    try
    {
      using UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl + "/auth/verify");
      webRequest.SetRequestHeader("Cookie", $"session={sessionToken}");

      await webRequest.SendWebRequest();

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
        string errorMessage = $"Verification failed: {webRequest.error} (Status: {webRequest.responseCode})";
        Debug.LogError(errorMessage);
        callback?.Invoke(false, null, errorMessage);
      }
      else
      {
        string jsonResponse = webRequest.downloadHandler.text;
        Debug.Log($"Verify Response: {jsonResponse}");

        UserResponse response = JsonUtility.FromJson<UserResponse>(jsonResponse);
        if (response != null && response.user != null)
        {
          user = response.user;
          Debug.Log("User verified successfully.");
          Save();
          callback?.Invoke(true, user, "User verified successfully");
        }
        else
        {
          string errorMessage = "Failed to parse user data.";
          Debug.LogError(errorMessage);
          callback?.Invoke(false, null, errorMessage);
        }
      }
    }
    catch (Exception ex)
    {
      string errorMessage = $"Exception during verification: {ex.Message}";
      Debug.LogError(errorMessage);
      callback?.Invoke(false, null, errorMessage);
    }
  }

  public async void Login(string usernameOrEmail, string password, LoginCallback callback = null)
  {
    try
    {
      WWWForm form = new();
      form.AddField("usernameOrEmail", usernameOrEmail);
      form.AddField("password", password);

      Debug.Log($"Attempting login to: {baseUrl}/auth/login");

      using UnityWebRequest webRequest = UnityWebRequest.Post(baseUrl + "/auth/login", form);
      webRequest.timeout = 10;

      await webRequest.SendWebRequest();

      Debug.Log($"Login response code: {webRequest.responseCode}");

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
        if (webRequest.responseCode == 401)
        {
          Debug.LogError("Unauthorized: Invalid username or password.");
          callback?.Invoke(false, "Invalid username or password.");
          return;
        }

        string errorMessage = $"Login failed: {webRequest.error} (Status: {webRequest.responseCode})";
        Debug.LogError(errorMessage);
        callback?.Invoke(false, errorMessage);
        return;
      }

      string jsonResponse = webRequest.downloadHandler.text;
      Debug.Log($"Login Response: {jsonResponse}");

      LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonResponse);
      if (response != null && !string.IsNullOrEmpty(response.token))
      {
        sessionToken = response.token;
        Save();
        Debug.Log("Login successful, token received.");
        callback?.Invoke(true, "Login successful");
        Verify();
      }
      else if (response != null && !string.IsNullOrEmpty(response.error))
      {
        Debug.LogError($"Login error: {response.error}");
        callback?.Invoke(false, response.error);
      }
      else
      {
        Debug.LogError("Failed to parse login response or token is missing.");
        callback?.Invoke(false, "Failed to parse login response");
      }
    }
    catch (Exception ex)
    {
      string errorMessage = $"Exception during login: {ex.Message}";
      Debug.LogError(errorMessage);
      callback?.Invoke(false, errorMessage);
    }

  }

  public async Task<bool> PullStats()
  {
    if (user == null)
    {
      Debug.LogWarning("User is not logged in. Cannot pull stats.");
      return false;
    }

    Debug.Log("Pulling stats from server...");

    try
    {
      using UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl + "/user/stats");
      webRequest.SetRequestHeader("Cookie", $"session={sessionToken}");

      await webRequest.SendWebRequest();

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Failed to pull stats: {webRequest.error} (Status: {webRequest.responseCode})");
        return false;
      }

      string jsonResponse = webRequest.downloadHandler.text;
      Debug.Log($"Pull Stats Response: {jsonResponse}");

      StatsData serverStats = JsonUtility.FromJson<StatsData>(jsonResponse);
      if (serverStats != null)
      {
        MergeStats(serverStats);
        Save();
        Debug.Log("Stats pulled and merged successfully.");
        return true;
      }
      else
      {
        Debug.LogError("Failed to parse stats data.");
        return false;
      }
    }
    catch (Exception e)
    {
      Debug.LogError($"Failed to pull stats: {e.Message}");
      return false;
    }
  }

  public async Task<bool> PushStats()
  {
    if (user == null)
    {
      Debug.LogWarning("User is not logged in. Cannot push stats.");
      return false;
    }

    Debug.Log("Pushing stats to server...");
    try
    {
      string jsonData = JsonUtility.ToJson(stats);
      Debug.Log($"Pushing stats: {jsonData}");

      using UnityWebRequest webRequest = UnityWebRequest.Put(baseUrl + "/user/stats", jsonData);
      webRequest.SetRequestHeader("Content-Type", "application/json");
      webRequest.SetRequestHeader("Cookie", $"session={sessionToken}");

      await webRequest.SendWebRequest();

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Failed to push stats: {webRequest.error} (Status: {webRequest.responseCode})");
        return false;
      }

      string jsonResponse = webRequest.downloadHandler.text;
      Debug.Log($"Push Stats Response: {jsonResponse}");

      StatsData serverStats = JsonUtility.FromJson<StatsData>(jsonResponse);
      if (serverStats != null)
      {
        MergeStats(serverStats);
        Save();
        Debug.Log("Stats pushed and updated from server successfully.");
      }

      return true;
    }
    catch (Exception e)
    {
      Debug.LogError($"Failed to push stats: {e.Message}");
      return false;
    }
  }

  private void MergeStats(StatsData serverStats)
  {
    stats.playTime = Math.Max(stats.playTime, serverStats.playTime);
    stats.totalDeaths = Math.Max(stats.totalDeaths, serverStats.totalDeaths);
    stats.totalKills = Math.Max(stats.totalKills, serverStats.totalKills);
    stats.totalItemsCollected = Math.Max(stats.totalItemsCollected, serverStats.totalItemsCollected);
    stats.furthestLevelReached = Math.Max(stats.furthestLevelReached, serverStats.furthestLevelReached);
    stats.highestSpeedStat = Math.Max(stats.highestSpeedStat, serverStats.highestSpeedStat);
  }


  [Serializable]
  public class OnlineUser
  {
    public string _id;
    public string username;
    public StatsData stats;
    public string thumbnail;
  }

  [Serializable]
  public class LoginResponse
  {
    public bool success;
    public string token;
    public string error;
  }

  [Serializable]
  public class UserResponse
  {
    public OnlineUser user;
  }
}