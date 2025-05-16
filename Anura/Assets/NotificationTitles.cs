using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationTitles : MonoBehaviour
{
    public static NotificationTitles Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float defaultDisplayDuration = 2.5f;
    [SerializeField] private float fadeDuration = 0.5f; // Duration for fade in/out

    private CanvasGroup canvasGroup;
    private Coroutine currentNotificationCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // If this UI is meant to persist across scenes, uncomment the next line
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Attempt to get TextMeshProUGUI from children
        if (notificationText == null)
        {
            notificationText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // Get or add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (notificationText == null)
        {
            Debug.LogError("NotificationTitles: TextMeshProUGUI child component not found!", this);
            enabled = false; // Disable script if no text component
            return;
        }
        
        canvasGroup.alpha = 0; // Start fully transparent
        notificationText.text = ""; // Clear any default text
    }

    public void ShowNotification(string message)
    {
        ShowNotification(message, defaultDisplayDuration);
    }

    public void ShowNotification(string message, float duration)
    {
        if (notificationText == null || canvasGroup == null) return;

        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }
        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, duration));
    }

    private IEnumerator ShowNotificationCoroutine(string message, float displayTime)
    {
        notificationText.text = message;

        // Fade In
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time if game can be paused
            yield return null;
        }
        canvasGroup.alpha = 1;

        // Hold duration (adjust for fade times)
        float holdTime = displayTime - (2 * fadeDuration);
        if (holdTime < 0) holdTime = 0; // Ensure hold time isn't negative
        yield return new WaitForSecondsRealtime(holdTime);

        // Fade Out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;

        notificationText.text = ""; // Clear text after fading out
        currentNotificationCoroutine = null;
    }
}