using UnityEngine;
using DG.Tweening;

public class HitEffect : MonoBehaviour
{
    [Header("Hit Effect Settings")]
    [Tooltip("The Renderer component that has the material with the hit effect shader.")]
    public Renderer targetRenderer; 
    [Tooltip("The maximum value for the _HitEffectAmount shader property.")]
    public float maxHitEffectAmount = 1f;
    [Tooltip("Duration for the hit effect to fade in (quickly).")]
    public float fadeInDuration = 0.1f;
    [Tooltip("Duration for the hit effect to fade out.")]
    public float fadeOutDuration = 0.4f;

    private Material hitEffectMaterial;
    private static readonly int HitEffectAmountID = Shader.PropertyToID("_HitEffectAmount");
    private Sequence hitEffectSequence;

    void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null)
        {
            // It's important to use a material instance to avoid changing the shared material asset
            hitEffectMaterial = targetRenderer.material; 
        }
        else
        {
            Debug.LogError("HitEffect: Target Renderer not found or assigned. Disabling script.", this);
            enabled = false;
            return;
        }

        // Initialize the effect amount to 0
        if (hitEffectMaterial != null)
        {
            hitEffectMaterial.SetFloat(HitEffectAmountID, 0f);
        }
    }

    /// <summary>
    /// Plays the hit effect.
    /// </summary>
    public void Play()
    {
        if (hitEffectMaterial == null) return;

        // Kill any existing hit effect tween to restart it
        if (hitEffectSequence != null && hitEffectSequence.IsActive())
        {
            hitEffectSequence.Kill();
        }

        // Ensure the effect starts from 0 if it was interrupted
        hitEffectMaterial.SetFloat(HitEffectAmountID, 0f);

        hitEffectSequence = DOTween.Sequence();
        // Fade In (quickly increase effect amount)
        // Changed Ease.OutCubic to Ease.OutExpo for a very fast initial burst
        hitEffectSequence.Append(hitEffectMaterial.DOFloat(maxHitEffectAmount, HitEffectAmountID, fadeInDuration).SetEase(Ease.OutExpo));
        // Fade Out (decrease effect amount back to 0)
        // Changed Ease.OutCubic to Ease.OutExpo for a very fast start to the fade-out
        hitEffectSequence.Append(hitEffectMaterial.DOFloat(0f, HitEffectAmountID, fadeOutDuration).SetEase(Ease.OutExpo));
        
        hitEffectSequence.Play();
    }

    void OnDestroy()
    {
        // Clean up the sequence when the object is destroyed
        if (hitEffectSequence != null)
        {
            hitEffectSequence.Kill();
        }
    }
}