using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "UserData", menuName = "Scriptable Objects/UserData")]
public class UserData : ScriptableObject
{
  private readonly string baseUrl = "https://anura.ameow.gay/api";
  private string sessionToken = string.Empty;
  public User user;

  // Add a delegate for login callbacks
  public delegate void LoginCallback(bool success, string message);
  public delegate void VerifyCallback(bool success, User user, string message);

  public async void Verify(VerifyCallback callback = null)
  {
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

  [Serializable]
  public class User
  {
    public string _id;
    public string username;
    public List<string> achievements;
    public int playTime;
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
    public User user;
  }
}