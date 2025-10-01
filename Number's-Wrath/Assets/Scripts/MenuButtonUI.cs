using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button), typeof(Image))]
public class MenuButtonUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Visual")]
    public Color baseColor = Color.black;
    public Color glowColor = new Color(0f, 1f, 0.95f);
    public float glowSpeed = 8f;
    public float hoverAlpha = 0.15f;

    [Header("Scale Press")]
    public Vector3 pressedScale = new Vector3(0.94f, 0.94f, 0.94f);
    public float scaleSpeed = 12f;

    [Header("Title Pulse")]
    [Tooltip("Si está activo, este botón disparará un pulso en el título")]
    public bool triggerPulse = false;
    public float pulseIntensity = 1.6f;
    public TitlePulse titlePulseReference;

    [Header("Audio (opcional)")]
    public AudioClip hoverSfx;
    public AudioClip clickSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    [Header("On Click (connect here)")]
    public UnityEvent onClickEvent;

    Image image;
    bool hovering = false;
    bool pressed = false;
    Vector3 initialScale;

    AudioSource localAudio;

    void Awake()
    {
        image = GetComponent<Image>();
        image.color = baseColor;
        initialScale = transform.localScale;

        if (hoverSfx != null || clickSfx != null)
        {
            localAudio = gameObject.AddComponent<AudioSource>();
            localAudio.playOnAwake = false;
            localAudio.volume = sfxVolume;
        }

        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnButtonClickedInternal);

        if (titlePulseReference == null)
        {
            titlePulseReference = FindObjectOfType<TitlePulse>();
        }
    }

    void Update()
    {
        Color target = hovering ? glowColor : baseColor;
        if (hovering)
            target = Color.Lerp(target, Color.white, hoverAlpha);
        image.color = Color.Lerp(image.color, target, Time.deltaTime * glowSpeed);

        Vector3 targetScale = pressed ? pressedScale : initialScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        if (hoverSfx != null)
        {
            if (SoundManager.InstanceExists)
                SoundManager.Instance.PlaySFX(hoverSfx);
            else
                localAudio?.PlayOneShot(hoverSfx, sfxVolume);
        }

        if (triggerPulse && titlePulseReference != null)
        {
            titlePulseReference.DoQuickPulse(pulseIntensity);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    void OnButtonClickedInternal()
    {
        if (clickSfx != null)
        {
            if (SoundManager.InstanceExists)
                SoundManager.Instance.PlaySFX(clickSfx);
            else
                localAudio?.PlayOneShot(clickSfx, sfxVolume);
        }

        onClickEvent?.Invoke();
    }
}
