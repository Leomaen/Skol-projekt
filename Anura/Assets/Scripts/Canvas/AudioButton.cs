using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private bool playClickSound = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound && button != null && button.interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound && button != null && button.interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
