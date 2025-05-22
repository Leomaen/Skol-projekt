using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }

    [Header("Music Audio Sources")]
    [SerializeField] private List<AudioSource> backgroundMusic;
    [SerializeField] private AudioSource bossMusic;
    private AudioSource currentBackgroundMusic;

    [Header("UI Audio Sources")]
    [SerializeField] private AudioSource buttonClick;
    [SerializeField] private AudioSource buttonHover;
    [SerializeField] private AudioSource menuOpen;
    [SerializeField] private AudioSource menuClose;

    [Header("Game Sounds")]
    [SerializeField] private List<SoundEffect> gameSounds;
    private readonly Dictionary<string, AudioSource> soundEffects = new();

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioSource source;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundEffects();
            PlayBackgroundMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSoundEffects()
    {
        foreach (var sound in gameSounds)
        {
            if (sound.source != null)
            {
                soundEffects[sound.name] = sound.source;
            }
        }
    }

    // Music Control Methods
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || backgroundMusic.Count == 0) return;

        if (currentBackgroundMusic != null)
        {
            currentBackgroundMusic.Stop();
        }

        int randomIndex = Random.Range(0, backgroundMusic.Count);
        currentBackgroundMusic = backgroundMusic[randomIndex];

        if (currentBackgroundMusic != null)
        {
            currentBackgroundMusic.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (currentBackgroundMusic != null)
        {
            currentBackgroundMusic.Stop();
        }
    }

    public void PlayBossMusic()
    {
        StopBackgroundMusic();

        if (bossMusic != null)
        {
            bossMusic.Play();
        }
    }

    public void StopBossMusic()
    {
        if (bossMusic != null)
        {
            bossMusic.Stop();
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClick != null)
        {
            buttonClick.Play();
        }
    }

    public void PlayButtonHover()
    {
        if (buttonHover != null)
        {
            buttonHover.Play();
        }
    }

    public void PlayMenuOpen()
    {
        if (menuOpen != null)
        {
            menuOpen.Play();
        }
    }

    public void PlayMenuClose()
    {
        if (menuClose != null)
        {
            menuClose.Play();
        }
    }

    // Game Sound Methods
    public void PlaySound(string soundName)
    {
        if (soundEffects.TryGetValue(soundName, out AudioSource source))
        {
            source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound {soundName} not found in AudioManager");
        }
    }

    public void StopSound(string soundName)
    {
        if (soundEffects.TryGetValue(soundName, out AudioSource source))
        {
            source.Stop();
        }
    }
}