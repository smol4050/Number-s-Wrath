using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform spawnParent;         // NumbersContainer (RectTransform)
    [SerializeField] FloatingNumber digitPrefab;        // prefab con FloatingNumber + Text

    [Header("Pool")]
    [SerializeField] int poolSize = 40;

    [Header("Spawn Settings")]
    [SerializeField] float spawnInterval = 0.12f;
    [SerializeField] float minSpeed = 50f;
    [SerializeField] float maxSpeed = 140f;
    [SerializeField] float minLifetime = 2f;
    [SerializeField] float maxLifetime = 5f;
    [SerializeField] int minFont = 18;
    [SerializeField] int maxFont = 36;
    [SerializeField] Color colorLow = new Color(0.3f, 1f, 0.5f); // verde oscuro
    [SerializeField] Color colorHigh = new Color(0.6f, 1f, 0.9f); // verde claro
    [SerializeField] float swayAmp = 14f;
    [SerializeField] float swayFreq = 1.1f;

    List<FloatingNumber> pool;
    Coroutine spawnCoroutine;

    void Awake()
    {
        if (spawnParent == null)
            spawnParent = GetComponent<RectTransform>();

        pool = new List<FloatingNumber>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(digitPrefab.gameObject, spawnParent);
            go.SetActive(false);
            pool.Add(go.GetComponent<FloatingNumber>());
        }
    }

    void OnEnable()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne()
    {
        FloatingNumber item = GetPooled();
        if (item == null) return;

        int digit = Random.Range(0, 10);
        float speed = Random.Range(minSpeed, maxSpeed);
        float life = Random.Range(minLifetime, maxLifetime);
        int font = Random.Range(minFont, maxFont + 1);
        Color color = Color.Lerp(colorLow, colorHigh, Random.value);

        float halfW = spawnParent.rect.width * 0.5f;
        float halfH = spawnParent.rect.height * 0.5f;
        float x = Random.Range(-halfW, halfW);
        float y = halfH + 40f;

        Vector2 startPos = new Vector2(x, y);

        float localSwayAmp = swayAmp * (0.6f + Random.value * 0.8f);
        float localSwayFreq = swayFreq * (0.8f + Random.value * 0.9f);
        item.Init(digit.ToString(), startPos, speed, life, font, color, localSwayAmp, localSwayFreq);
    }

    FloatingNumber GetPooled()
    {
        for (int i = 0; i < pool.Count; i++)
            if (!pool[i].gameObject.activeInHierarchy)
                return pool[i];
        return null;
    }
}
