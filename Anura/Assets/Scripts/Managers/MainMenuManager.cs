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

        statisticsText.text = JsonUtility.ToJson(userData.stats, true);
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
}
