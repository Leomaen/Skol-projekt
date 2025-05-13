using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameSceneName = "SampleScene";
    [SerializeField] Button loadGameButton;
    public GameState gameState;
    public UserData userData;
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
                await userData.PullStats();
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
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnLoadGameButtonClicked()
    {
        gameState.Load();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSignInPanel()
    {
        Debug.Log("Opening Sign In Form...");
        signInPanel.SetActive(true);
    }

    public void CloseSignInPanel()
    {
        Debug.Log("Closing Sign In Form...");
        signInPanel.SetActive(false);
    }

    public void OnSubmitSignInButtonClicked()
    {
        Debug.Log("Submitting Sign In Form...");
        string usernameOrEmail = userNameInputField.text;
        string password = passwordInputField.text;

        userData.Login(usernameOrEmail, password, async (success, message) =>
        {
            Debug.Log(message);
            if (success)
            {
                await userData.PullStats();
                signInButton.SetActive(false);
                signOutButton.SetActive(true);
                signInPanel.SetActive(false);
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
