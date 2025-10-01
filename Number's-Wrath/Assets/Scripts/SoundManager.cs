using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public static bool InstanceExists => Instance != null;
    
    public static event Action<AudioClip> OnPlayMusicRequested;
    public static event Action<AudioClip, float> OnPlaySFXRequested;
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;

    [Header("Default Clips (assign here)")]
    public AudioClip defaultMusicClip;
    public AudioClip clickClip;
    public AudioClip testSfxClip;

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultMusicVolume = 0.6f;
    [Range(0f, 1f)] public float defaultSfxVolume = 0.8f;

    const string KEY_MUSIC = "MusicVolume";
    const string KEY_SFX = "SFXVolume";

    float musicVolume;
    float sfxVolume;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumes();

        if (defaultMusicClip != null)
            PlayMusic(defaultMusicClip);
    }

    void LoadVolumes()
    {
        musicVolume = PlayerPrefs.HasKey(KEY_MUSIC) ? PlayerPrefs.GetFloat(KEY_MUSIC) : defaultMusicVolume;
        sfxVolume = PlayerPrefs.HasKey(KEY_SFX) ? PlayerPrefs.GetFloat(KEY_SFX) : defaultSfxVolume;
        
        OnMusicVolumeChanged?.Invoke(musicVolume);
        OnSFXVolumeChanged?.Invoke(sfxVolume);
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_MUSIC, musicVolume);
        PlayerPrefs.Save();
        OnMusicVolumeChanged?.Invoke(musicVolume);
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_SFX, sfxVolume);
        PlayerPrefs.Save();
        OnSFXVolumeChanged?.Invoke(sfxVolume);
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    
    public void PlayMusic(AudioClip clip, float fadeTime = 0.2f)
    {
        if (clip == null) return;
        OnPlayMusicRequested?.Invoke(clip);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        OnPlaySFXRequested?.Invoke(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlayClick() => PlaySFX(clickClip);
}
