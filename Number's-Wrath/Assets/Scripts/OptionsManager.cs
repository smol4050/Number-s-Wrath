using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    [Header("UI")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Button backButton;

    [Header("Scene names")]
    public string menuSceneName = "MainMenu";

    public AudioClip demoSfxClip;

    void Start()
    {
        float m = SoundManager.InstanceExists ? SoundManager.Instance.GetMusicVolume() : 0.6f;
        float s = SoundManager.InstanceExists ? SoundManager.Instance.GetSFXVolume() : 0.8f;

        if (musicSlider != null) musicSlider.value = m;
        if (sfxSlider != null) sfxSlider.value = s;

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
    }

    void OnMusicChanged(float value)
    {
        if (SoundManager.InstanceExists)
            SoundManager.Instance.SetMusicVolume(value);
    }

    void OnSfxChanged(float value)
    {
        if (SoundManager.InstanceExists)
            SoundManager.Instance.SetSFXVolume(value);

        AudioClip clip = demoSfxClip != null ? demoSfxClip : (SoundManager.InstanceExists ? SoundManager.Instance.testSfxClip : null);
        if (clip != null)
            SoundManager.Instance.PlaySFX(clip, 1f);
    }

    void OnBackPressed()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    void OnDestroy()
    {
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        if (backButton != null) backButton.onClick.RemoveListener(OnBackPressed);
    }
}
