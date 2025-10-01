using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        SoundManager.OnPlaySFXRequested += HandlePlaySFX;
        SoundManager.OnSFXVolumeChanged += HandleSFXVolumeChanged;
    }

    void OnDisable()
    {
        SoundManager.OnPlaySFXRequested -= HandlePlaySFX;
        SoundManager.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
    }

    void Start()
    {
        if (SoundManager.InstanceExists)
            audioSource.volume = SoundManager.Instance.GetSFXVolume();
    }

    void HandlePlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;
        float volume = (SoundManager.InstanceExists ? SoundManager.Instance.GetSFXVolume() : 1f) * volumeScale;
        audioSource.PlayOneShot(clip, volume);
    }

    void HandleSFXVolumeChanged(float newVol)
    {
        audioSource.volume = newVol;
    }
}
