using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SceneFader : MonoBehaviour
{
    public float FadeDuration = 1f;
    public FadeType CurrentFadeType;

    private int _fadeAmount = Shader.PropertyToID("_FadeAmount");
    
    private int _usePlainBlack = Shader.PropertyToID("_UsePlainBlack");

    private int _useGoop = Shader.PropertyToID("_UseGoop");

    private int? _lastEffect;

    private UnityEngine.UI.Image _image;
    private Material _material;

    public enum FadeType
    {
        PlainBlack,
        Goop
    }

    private void Awake()
    {
        _image = GetComponent<UnityEngine.UI.Image>();

        Material mat = _image.material;
        _image.material = new Material(mat);
        _material = _image.material;

        _lastEffect = _usePlainBlack;
    }

    private void Update()
    {
        if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            FadeOut(CurrentFadeType);
        }

        if (Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            FadeIn(CurrentFadeType);
        }
    }

    public void FadeOut(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeOut();
    }

    public void FadeIn(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeIn();
    }

    private void ChangeFadeEffect(FadeType fadeType)
    {
        if (_lastEffect.HasValue)
        {
            _material.SetFloat(_lastEffect.Value, 0f);
        }

        switch (fadeType)
        {
            case FadeType.PlainBlack:
                SwitchEffect(_usePlainBlack);
                break;

            case FadeType.Goop:
                SwitchEffect(_useGoop);
                break;
        }
    }

    private void SwitchEffect(int effectToTurnOn)
    {
        _material.SetFloat(effectToTurnOn, 1f);

        _lastEffect = effectToTurnOn;
    }

    private void StartFadeIn()
    {
        _material.SetFloat(_fadeAmount, 1f);

        _material.DOFloat(0f, _fadeAmount, FadeDuration)
                .SetEase(Ease.InOutSine);
    }

    private void StartFadeOut()
    {
        _material.SetFloat(_fadeAmount, 0f);

        _material.DOFloat(1f, _fadeAmount, FadeDuration)
                .SetEase(Ease.InOutSine);
    }
}
