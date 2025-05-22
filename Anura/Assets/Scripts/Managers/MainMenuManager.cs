using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public GameState gameState;
    public UserData userData;

    [SerializeField] string gameSceneName = "SampleScene";
    [SerializeField] Button loadGameButton;
    [SerializeField] SceneFader sceneFader;
    public GameObject statisticsPanel;
    public TMP_Text statisticsText;

    public GameObject signInPanel;
    public TMP_InputField userNameInputField;
    public TMP_InputField passwordInputField;
    public GameObject signInButton;
    public GameObject signOutButton;
    public TMP_Text signInErrorText;
    void Start()
    {
        Time.timeScale = 1f;
        if (gameState.HasSave())
        {
            loadGameButton.interactable = true;
        }
        else
        {
            loadGameButton.interactable = false;
        }

        userData.Load();

        userData.Verify(async (success, user, message) =>
        {
            if (success)
            {
                Debug.Log("User verified successfully.");
                await userData.PushStats();
                signInButton.SetActive(false);
                signOutButton.SetActive(true);
            }
            else
            {
                Debug.Log("User verification failed.");
                signInButton.SetActive(true);
                signOutButton.SetActive(false);
            }
        });
    }

    public void OnNewGameButtonClicked()
    {
        gameState.NewGame();
        sceneFader.FadeOut(SceneFader.FadeType.Goop, () =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }

    public void OnLoadGameButtonClicked()
    {
        gameState.Load();
        sceneFader.FadeOut(SceneFader.FadeType.Goop, () =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }

    public async void OpenStatisticsPanel()
    {
        Debug.Log("Opening Statistics Panel...");
        await userData.PushStats();
        AudioManager.Instance.PlayMenuOpen();
        statisticsPanel.SetActive(true);

        statisticsText.text = $"Total Playtime: {StringifyTimeDelta(userData.stats.playTime)}\n" +
            $"Total Deaths: {userData.stats.totalDeaths}\n" +
            $"Total Kills: {userData.stats.totalKills}\n" +
            $"Total Items Collected: {userData.stats.totalItemsCollected}\n" +
            $"Furthest Floor Reached: {userData.stats.furthestLevelReached}\n";
    }

    public void CloseStatisticsPanel()
    {
        Debug.Log("Closing Statistics Panel...");
        AudioManager.Instance.PlayMenuClose();
        statisticsPanel.SetActive(false);
    }

    public void OpenSignInPanel()
    {
        Debug.Log("Opening Sign In Form...");
        AudioManager.Instance.PlayMenuOpen();
        signInPanel.SetActive(true);
    }

    public void CloseSignInPanel()
    {
        Debug.Log("Closing Sign In Form...");
        AudioManager.Instance.PlayMenuClose();
        signInPanel.SetActive(false);
    }

    public void OnSubmitSignInButtonClicked()
    {
        Debug.Log("Submitting Sign In Form...");
        string usernameOrEmail = userNameInputField.text;
        string password = passwordInputField.text;

        userData.Login(usernameOrEmail, password, (success, message) =>
        {
            Debug.Log(message);
            if (success)
            {
                signInButton.SetActive(false);
                signOutButton.SetActive(true);
                CloseSignInPanel();
            }
            else
            {
                Debug.Log("Login failed. Please try again.");
                signInErrorText.gameObject.SetActive(true);
                signInErrorText.text = message;
            }
        });
    }


    public void OnSignOutButtonClicked()
    {
        Debug.Log("Signing Out...");
        userData.SignOut();
        signInButton.SetActive(true);
        signOutButton.SetActive(false);
    }

    public void OnQuitGameButtonClicked()
    {
        Debug.Log("Quiting Game...");
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private readonly struct TimeUnit
    {
        public string Label { get; }
        public long Milliseconds { get; }

        public TimeUnit(string label, long milliseconds)
        {
            Label = label;
            Milliseconds = milliseconds;
        }
    }

    private static readonly TimeUnit[] NamedUnits = new TimeUnit[]
    {
        new("year", 31536000000L),
        new("month", 2592000000L),
        new("day", 86400000L),
        new("hour", 3600000L),
        new("minute", 60000L),
        new("second", 1000L),
    };

    public static string StringifyTimeDelta(long deltaMilliseconds, int precision = 2, bool useBindingWords = false)
    {
        if (deltaMilliseconds < 1000) // Treat less than a second as "less than a second"
        {
            // Check if it's effectively zero or negative, adjust if needed or return specific message
            if (deltaMilliseconds <= 0) return "0 seconds"; // Or "just now", "less than a second ago" depending on context
            return "less than a second";
        }

        var parts = new System.Collections.Generic.List<string>();
        long remainingDelta = deltaMilliseconds;

        foreach (var unit in NamedUnits)
        {
            if (parts.Count >= precision) break;

            if (remainingDelta >= unit.Milliseconds)
            {
                long count = remainingDelta / unit.Milliseconds;
                parts.Add($"{count} {unit.Label}{(count != 1 ? "s" : "")}");
                remainingDelta %= unit.Milliseconds;
            }
        }

        if (parts.Count == 0) // Should only happen if precision is 0 or delta was < 1000 and not handled above
        {
            // Fallback for very small deltas if the initial check wasn't enough
            if (deltaMilliseconds > 0) return "less than a second";
            return "0 seconds"; // Or a more appropriate default
        }

        if (useBindingWords && parts.Count > 1)
        {
            if (parts.Count == 2)
            {
                return $"{parts[0]} and {parts[1]}";
            }
            else
            {
                string lastPart = parts[parts.Count - 1];
                parts.RemoveAt(parts.Count - 1);
                return $"{string.Join(", ", parts)}, and {lastPart}";
            }
        }
        else
        {
            return string.Join(" ", parts);
        }
    }
}
