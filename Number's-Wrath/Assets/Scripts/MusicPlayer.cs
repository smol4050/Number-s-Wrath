using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        SoundManager.OnPlayMusicRequested += HandlePlayMusic;
        SoundManager.OnMusicVolumeChanged += HandleMusicVolumeChanged;
    }

    void OnDisable()
    {
        SoundManager.OnPlayMusicRequested -= HandlePlayMusic;
        SoundManager.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
    }

    void Start()
    {
        if (SoundManager.InstanceExists)
        {
            audioSource.volume = SoundManager.Instance.GetMusicVolume();
            if (SoundManager.Instance.defaultMusicClip != null && (audioSource.clip == null || audioSource.clip != SoundManager.Instance.defaultMusicClip))
                HandlePlayMusic(SoundManager.Instance.defaultMusicClip);
        }
    }

    void HandlePlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    void HandleMusicVolumeChanged(float newVol)
    {
        audioSource.volume = newVol;
    }
}
