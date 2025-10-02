using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Top Left - Lives & Number")]
    [Tooltip("Prefab: root Image que contiene un child llamado 'Fill' (Image)")]
    public GameObject heartPrefabContainer;
    public Transform livesContainer;
    public Text powerNumberText;
    public float numberPulseDuration = 0.36f;
    public float numberPulseScale = 1.18f;

    [Header("Top Right - Kills")]
    public Text killsText;

    [Header("Bottom Right - Operations")]
    public Image plusButtonImage;
    public Image multButtonImage;

    int currentLives = 3;
    int maxLives = 3;
    int currentKills = 0;
    long currentNumber = 1;

    List<GameObject> heartInstances = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        RefreshLivesVisual();
        UpdatePowerNumber(currentNumber);
        UpdateKills(currentKills);
    }

    public void SetMaxLives(int max, int startLives = -1)
    {
        maxLives = Mathf.Max(1, max);
        if (startLives < 0) currentLives = maxLives;
        else currentLives = Mathf.Clamp(startLives, 0, maxLives);
        RefreshLivesVisual();
    }

    public void SetLives(int lives)
    {
        currentLives = Mathf.Clamp(lives, 0, maxLives);
        RefreshLivesVisual();
    }

    void RefreshLivesVisual()
    {
        foreach (var go in heartInstances) Destroy(go);
        heartInstances.Clear();
        
        for (int i = 0; i < maxLives; i++)
        {
            GameObject go = Instantiate(heartPrefabContainer, livesContainer);
            go.SetActive(true);
            
            Transform fill = go.transform.Find("Fill");
            if (fill != null)
            {
                fill.gameObject.SetActive(i < currentLives);
            }
            heartInstances.Add(go);
        }
    }

    public void UpdatePowerNumber(long num)
    {
        currentNumber = num;
        if (powerNumberText != null)
            powerNumberText.text = num.ToString();
        StopAllCoroutines();
        StartCoroutine(PulseNumberRoutine());
    }

    IEnumerator PulseNumberRoutine()
    {
        if (powerNumberText == null) yield break;
        float t = 0f;
        Vector3 baseScale = powerNumberText.transform.localScale;
        Vector3 target = baseScale * numberPulseScale;
        while (t < numberPulseDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin((t / numberPulseDuration) * Mathf.PI);
            powerNumberText.transform.localScale = Vector3.Lerp(baseScale, target, k);
            yield return null;
        }
        powerNumberText.transform.localScale = baseScale;
    }

    public void AddKill(int n = 1)
    {
        currentKills += n;
        UpdateKills(currentKills);
    }

    public void UpdateKills(int kills)
    {
        currentKills = kills;
        if (killsText != null)
            killsText.text = $"Kills: {currentKills:00}";
    }

    public void HighlightOperation(string op, float intensity = 1.12f, float dur = 0.12f)
    {
        if (op == "plus" && plusButtonImage != null)
        {
            StopCoroutine("HighlightRoutinePlus");
            StartCoroutine(HighlightRoutine(plusButtonImage, intensity, dur, "HighlightRoutinePlus"));
        }
        else if (op == "mult" && multButtonImage != null)
        {
            StopCoroutine("HighlightRoutineMult");
            StartCoroutine(HighlightRoutine(multButtonImage, intensity, dur, "HighlightRoutineMult"));
        }
    }

    IEnumerator HighlightRoutine(Image img, float intensity, float dur, string id)
    {
        if (img == null) yield break;
        Vector3 from = Vector3.one;
        Vector3 to = Vector3.one * intensity;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            img.transform.localScale = Vector3.Lerp(from, to, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            img.transform.localScale = Vector3.Lerp(to, from, t / dur);
            yield return null;
        }
        img.transform.localScale = from;
    }

    public void OnPlayerTookDamage(int newLives) => SetLives(newLives);
    public void OnPlayerCollectedNumber(long newNumber) => UpdatePowerNumber(newNumber);
    public void OnEnemyKilled() => AddKill(1);

    public bool TryHealOneLife()
    {
        if (currentLives >= maxLives) return false;
        currentLives = Mathf.Clamp(currentLives + 1, 0, maxLives);
        RefreshLivesVisual();
        return true;
    }
}
