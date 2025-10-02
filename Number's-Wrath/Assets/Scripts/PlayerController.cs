using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Player numbers & lives")]
    public long currentNumber = 2;
    public int maxLives = 3;
    public int initialLives = 3;
    int currentLives;

    [Header("Input keys")]
    public KeyCode plusKey = KeyCode.Q;
    public KeyCode multKey = KeyCode.E;
    public KeyCode healKey = KeyCode.F;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        currentLives = Mathf.Clamp(initialLives, 0, maxLives);

        GameUIManager.Instance?.SetMaxLives(maxLives, currentLives);
        GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
    }

    void Update()
    {
        if (Input.GetKeyDown(plusKey))
        {
            OnOperationPlusPressed();
        }
        if (Input.GetKeyDown(multKey))
        {
            OnOperationMultPressed();
        }
        if (Input.GetKeyDown(healKey))
        {
            TryHealUsingNumber();
        }
    }

    public void OnOperationPlusPressed()
    {
        currentNumber += 1;
        GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
    }

    public void OnOperationMultPressed()
    {
        currentNumber *= 2;
        GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
    }

    public void TakeDamage(int dmg)
    {
        currentLives = Mathf.Clamp(currentLives - dmg, 0, maxLives);
        GameUIManager.Instance?.OnPlayerTookDamage(currentLives);
        if (currentLives <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Player Died");
    }

    public void TryHealUsingNumber()
    {
        if (currentNumber <= 0) return;

        if (currentLives >= maxLives)
        {
            return;
        }

        //currentNumber = Mathf.Max(0, currentNumber - 1);
        currentLives = Mathf.Clamp(currentLives + 1, 0, maxLives);

        GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
        GameUIManager.Instance?.OnPlayerTookDamage(currentLives);
    }
}
