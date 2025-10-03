using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Numbers & Lives")]
    public long currentNumber = 2;
    public int maxLives = 3;
    public int initialLives = 3;
    int currentLives;

    [Header("Multiply tracking")]
    [HideInInspector] public int multiplyCount = 0;

    [Header("Heal rules")]
    public bool applyMultiplyPenalty = true;
    public int multiplyCountThreshold = 10;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform feetPoint;
    public float feetCheckRadius = 0.12f;

    [Header("Input keys")]
    public KeyCode plusKey = KeyCode.Q;
    public KeyCode multKey = KeyCode.E;
    public KeyCode healKey = KeyCode.H;

    Rigidbody2D rb;
    Collider2D col;

    bool facingRight = true;
    bool isGrounded = false;
    float horizontalInput = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        currentLives = Mathf.Clamp(initialLives, 0, maxLives);
        GameUIManager.Instance?.SetMaxLives(maxLives, currentLives);
        GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
        {
            TryJump();
        }

        if (Input.GetKeyDown(plusKey))
        {
            OnOperationPlusPressed();
            
            GameUIManager.Instance?.HighlightOperation("plus");
            
            if (SoundManager.InstanceExists) SoundManager.Instance.PlayClick();
        }

        if (Input.GetKeyDown(multKey))
        {
            OnOperationMultPressed();
            GameUIManager.Instance?.HighlightOperation("mult");
            if (SoundManager.InstanceExists) SoundManager.Instance.PlayClick();
        }

        if (Input.GetKeyDown(healKey))
        {
            bool ok = TryHealUsingNumber();
            if (ok)
            {
                if (SoundManager.InstanceExists) SoundManager.Instance.PlaySFX(SoundManager.Instance.testSfxClip);
            }
        }

        CheckGrounded();
    }

    void FixedUpdate()
    {
        MoveHorizontal(horizontalInput);
    }

    void MoveHorizontal(float input)
    {
        float vx = input * moveSpeed;
        rb.velocity = new Vector2(vx, rb.velocity.y);

        if (input > 0 && !facingRight) Flip();
        else if (input < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    void TryJump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void CheckGrounded()
    {
        if (feetPoint == null)
        {
            Bounds b = col.bounds;
            Vector2 origin = new Vector2(b.center.x, b.min.y - 0.05f);
            isGrounded = Physics2D.OverlapCircle(origin, feetCheckRadius, groundLayer) != null;
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(feetPoint.position, feetCheckRadius, groundLayer) != null;
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
        multiplyCount++;
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
        Debug.Log("Player died");
    }

    public bool TryHealUsingNumber()
    {
        if (currentLives >= maxLives) return false;

        int baseCost = ComputeBaseHealCost(currentLives);
        int extraCost = 0;

        if (applyMultiplyPenalty && multiplyCount > multiplyCountThreshold)
        {
            float halfMult = 0.5f * multiplyCount;
            float tenPercentLives = 0.1f * currentLives;
            float rawExtra = halfMult * tenPercentLives;
            extraCost = Mathf.CeilToInt(rawExtra);
        }

        int totalCost = Mathf.Max(1, baseCost + extraCost);

        if (currentNumber >= totalCost)
        {
            currentNumber -= totalCost;
            currentLives = Mathf.Clamp(currentLives + 1, 0, maxLives);

            GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
            GameUIManager.Instance?.OnPlayerTookDamage(currentLives);

            return true;
        }

        return false;
    }

    int ComputeBaseHealCost(int lives)
    {
        if (lives >= 30) return 3;
        if (lives >= 20) return 2;
        return 1;
    }

    public void PlaceAt(Vector2 worldPos)
    {
        transform.position = worldPos;
        
        if (rb != null) rb.velocity = Vector2.zero;
    }
}
