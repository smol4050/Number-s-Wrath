using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TitlePulse : MonoBehaviour
{
    public float baseScale = 1f;
    public float pulseAmount = 0.04f;
    public float pulseSpeed = 1.8f;

    Text txt;
    float time;

    void Awake()
    {
        txt = GetComponent<Text>();
    }

    void Update()
    {
        time += Time.deltaTime * pulseSpeed;
        float s = baseScale + Mathf.Sin(time) * pulseAmount;
        transform.localScale = new Vector3(s, s, 1f);
    }

    public void DoQuickPulse(float intensity = 1.6f)
    {
        StopAllCoroutines();
        StartCoroutine(QuickPulseCoroutine(intensity));
    }

    System.Collections.IEnumerator QuickPulseCoroutine(float intensity)
    {
        float start = 1f;
        float target = 1f + pulseAmount * intensity;
        float t = 0f;
        float dur = 0.18f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(start, target, Mathf.Sin((t / dur) * Mathf.PI * 0.5f));
            transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }
}
