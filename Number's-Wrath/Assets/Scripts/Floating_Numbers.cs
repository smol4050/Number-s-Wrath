using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Text))]
public class FloatingNumber : MonoBehaviour
{
    RectTransform rt;
    Text txt;

    Color baseColor;
    float speed;
    float lifetime;
    float elapsed;
    float fadeDuration = 0.5f;

    float initialX;
    float swayAmplitude;
    float swayFrequency;
    bool running = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        txt = GetComponent<Text>();
    }

    // Inicializa la cifra cuando la saca del pool
    public void Init(string character, Vector2 anchoredStartPos, float speed, float lifetime, int fontSize, Color color, float swayAmp = 12f, float swayFreq = 1.2f)
    {
        gameObject.SetActive(true);
        txt.text = character;
        txt.fontSize = fontSize;
        baseColor = color;
        txt.color = baseColor;

        this.speed = speed;
        this.lifetime = lifetime;
        this.fadeDuration = Mathf.Min(0.5f, lifetime * 0.2f);

        rt.anchoredPosition = anchoredStartPos;
        initialX = anchoredStartPos.x;

        swayAmplitude = swayAmp;
        swayFrequency = swayFreq;

        elapsed = 0f;
        running = true;
    }

    void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;

        // Movimiento vertical
        float newY = rt.anchoredPosition.y - speed * Time.deltaTime;

        // Sway horizontal (no acumulativo)
        float swayX = initialX + Mathf.Sin(elapsed * swayFrequency) * swayAmplitude;

        rt.anchoredPosition = new Vector2(swayX, newY);

        // Fade out en los últimos segundos
        if (elapsed > lifetime - fadeDuration)
        {
            float fadeT = Mathf.InverseLerp(lifetime - fadeDuration, lifetime, elapsed);
            Color c = baseColor;
            c.a = Mathf.Lerp(1f, 0f, fadeT);
            txt.color = c;
        }

        if (elapsed >= lifetime)
            Deactivate();
    }

    void Deactivate()
    {
        running = false;
        gameObject.SetActive(false);
    }
}
