using System;
using System.Collections;
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

    [Header("Combat")]
    public Transform attackPoint;
    public float attackRange = 0.9f;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.5f;
    public float attackDelay = 0.15f;
    bool canAttack = true;

    [Header("Input keys")]
    public KeyCode plusKey = KeyCode.Q;
    public KeyCode multKey = KeyCode.E;
    public KeyCode healKey = KeyCode.H;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform feetPoint;
    public float feetCheckRadius = 0.12f;

    [Header("Animator")]
    public Animator animator;
    public string animParamIsWalking = "isWalking";
    public string animParamAttackTrigger = "Attack";

    [HideInInspector] public int multiplyCount = 0;

    long pendingRewardNumber = 0;
    bool hasPendingReward = false;

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
        UpdateAnimatorMovement();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) TryJump();

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }

        if (hasPendingReward)
        {
            if (Input.GetKeyDown(plusKey))
            {
                ApplyPendingRewardAsSum();
            }
            else if (Input.GetKeyDown(multKey))
            {
                ApplyPendingRewardAsMultiply();
            }
        }

        if (Input.GetKeyDown(healKey))
        {
            TryHealUsingNumber();
        }

        CheckGrounded();
    }

    void FixedUpdate()
    {
        MoveHorizontal(horizontalInput);
    }

    #region Movement & Animations
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

    void UpdateAnimatorMovement()
    {
        if (animator != null)
        {
            bool walking = Mathf.Abs(horizontalInput) > 0.1f && isGrounded;
            animator.SetBool(animParamIsWalking, walking);
        }
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
    #endregion

    #region Attack & Damage
    void TryAttack()
    {
        if (!canAttack) return;
        if (attackPoint == null)
        {
            Debug.LogWarning("[PlayerController] attackPoint no asignado.");
            return;
        }
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;

        if (animator != null) animator.SetTrigger(animParamAttackTrigger);

        yield return new WaitForSeconds(attackDelay);

        long damage = System.Math.Max(1, currentNumber / 2);

        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)attackPoint.position, attackRange, enemyLayer.value);
        bool anyHitAndDamaged = false;

        if (hits.Length == 0)
        {
            Debug.Log("Attack: no enemies in range.");
        }

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            float failProb = 0f;
            if (enemy.enemyNumber < currentNumber) failProb = 0f;
            else if (enemy.enemyNumber == currentNumber) failProb = 0.5f;
            else if (enemy.enemyNumber > currentNumber) failProb = 1f;

            float roll = UnityEngine.Random.value; // 0..1
            if (roll < failProb)
            {
                Debug.Log($"Attack failed on {enemy.name} (enemyNumber {enemy.enemyNumber}, playerNumber {currentNumber})");
                continue;
            }

            enemy.TakeDamage(damage, this);
            anyHitAndDamaged = true;
        }

        if (!anyHitAndDamaged)
        {
            Debug.Log("Attack: no enemies damaged by this attack.");
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
    #endregion

    #region Pending reward handling
    public void OnEnemyDefeatedSetPending(long enemyNumber)
    {
        if (hasPendingReward) return;

        hasPendingReward = true;
        pendingRewardNumber = enemyNumber;
        Debug.Log($"Pending reward set: {pendingRewardNumber}");

        GameUIManager.Instance?.UpdatePendingUI(true);
    }


    void ApplyPendingRewardAsSum()
    {
        if (!hasPendingReward) return;
        currentNumber = currentNumber + pendingRewardNumber;
        hasPendingReward = false;
        pendingRewardNumber = 0;
        GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
        Debug.Log("Applied pending as SUM");

        GameUIManager.Instance?.UpdatePendingUI(false);
    }

    void ApplyPendingRewardAsMultiply()
    {
        if (!hasPendingReward) return;
        currentNumber = currentNumber * pendingRewardNumber;
        multiplyCount++;
        hasPendingReward = false;
        pendingRewardNumber = 0;
        GameUIManager.Instance?.OnPlayerCollectedNumber(currentNumber);
        Debug.Log("Applied pending as MULTIPLY");

        GameUIManager.Instance?.UpdatePendingUI(false);
    }

    #endregion

    #region Heal logic (previously given)
    public bool TryHealUsingNumber()
    {
        if (currentLives >= maxLives) return false;
        int baseCost = ComputeBaseHealCost(currentLives);
        int extraCost = 0;
        if (multiplyCount > 10)
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
            if (SoundManager.InstanceExists) SoundManager.Instance.PlaySFX(SoundManager.Instance.testSfxClip);
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
    #endregion

    public bool HasPendingReward()
    {
        return hasPendingReward;
    }

    public void ApplyPendingAsSum()
    {
        if (!hasPendingReward) return;
        ApplyPendingRewardAsSum();
    }

    public void ApplyPendingAsMultiply()
    {
        if (!hasPendingReward) return;
        ApplyPendingRewardAsMultiply();
    }

}
