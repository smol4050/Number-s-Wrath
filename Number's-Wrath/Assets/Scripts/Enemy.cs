using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public long enemyNumber = 1;
    int maxHealth;
    int currentHealth;

    [Header("UI")]
    public Text numberText;

    [Header("AI")]
    public float moveSpeed = 3f;
    public float detectionRange = 8f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.2f;
    public float attackDuration = 0.6f;
    public int attackDamage = 1;
    public float fleeSpeedMultiplier = 1.0f;

    [Header("Knockback")]
    public float knockbackForce = 4f;
    public float knockbackDuration = 0.15f;

    [Header("Animator param names")]
    public Animator animator;
    public string animRun = "Run";
    public string animHit = "Hit";
    public string animDeath = "Death";
    public string animAttack = "Atack";
    public string animAttack2 = "Atack 2";

    Rigidbody2D rb;
    Transform player;
    float lastAttackTime = -999f;
    bool isAttacking = false;
    bool isDead = false;
    bool isKnockedback = false;

    Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance?.transform;

        maxHealth = (int)(2 + enemyNumber);
        currentHealth = maxHealth;

        if (numberText != null)
            numberText.text = enemyNumber.ToString();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isDead || isKnockedback) return;
        if (player == null)
        {
            player = PlayerController.Instance?.transform;
            if (player == null) return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > detectionRange)
        {
            StopMoving();
            SetAnimRun(false);
            return;
        }

        if (enemyNumber < PlayerController.Instance.currentNumber)
        {
            FleeFromPlayer();
        }
        else
        {
            if (dist > attackRange)
            {
                ChasePlayer();
            }
            else
            {
                TryAttack();
            }
        }
    }

    void StopMoving()
    {
        if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    void SetAnimRun(bool val)
    {
        if (animator != null && !string.IsNullOrEmpty(animRun))
            animator.SetBool(animRun, val);
    }

    void FleeFromPlayer()
    {
        if (player == null) return;

        Vector2 dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        float speed = moveSpeed * fleeSpeedMultiplier;
        if (rb != null) rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);

        SetAnimRun(true);

        FlipSprite(dir.x);
    }

    void ChasePlayer()
    {
        if (player == null) return;

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        if (rb != null) rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);

        SetAnimRun(true);

        FlipSprite(dir.x);
    }

    void FlipSprite(float dirX)
    {
        if (dirX > 0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = originalScale;
    }

    void TryAttack()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        SetAnimRun(false);

        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        StartCoroutine(DoAttackRoutine());
    }

    IEnumerator DoAttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        bool useSecond = false;
        if (!string.IsNullOrEmpty(animAttack2))
        {
            useSecond = Random.value < 0.3f;
        }

        if (animator != null)
        {
            if (useSecond) animator.SetBool(animAttack2, true);
            else animator.SetBool(animAttack, true);
        }

        float hitTime = attackDuration * 0.5f;
        yield return new WaitForSeconds(hitTime);

        if (PlayerController.Instance != null)
        {
            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (dist <= attackRange + 0.2f)
            {
                PlayerController.Instance.ReceiveDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitTime));

        if (animator != null)
        {
            if (useSecond) animator.SetBool(animAttack2, false);
            else animator.SetBool(animAttack, false);
        }

        isAttacking = false;
    }

    public void TakeDamage(long damage, PlayerController playerController)
    {
        if (isDead) return;

        currentHealth -= (int)damage;
        Debug.Log($"{name} took {damage} dmg. HP: {currentHealth}/{maxHealth}");

        if (animator != null && !string.IsNullOrEmpty(animHit))
        {
            animator.SetBool(animHit, true);
            StartCoroutine(ResetBool(animHit, 0.25f));
        }

        if (playerController != null)
            StartCoroutine(DoKnockback(playerController.transform));

        if (currentHealth <= 0)
        {
            Die(playerController);
        }
    }

    IEnumerator ResetBool(string param, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null && !string.IsNullOrEmpty(param))
            animator.SetBool(param, false);
    }

    IEnumerator DoKnockback(Transform source)
    {
        isKnockedback = true;

        if (rb != null)
        {
            float dirX = Mathf.Sign(transform.position.x - source.position.x);
            Vector2 knockDir = new Vector2(dirX, 0f).normalized;

            rb.velocity = Vector2.zero;

            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        isKnockedback = false;
    }



    void Die(PlayerController killer)
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"Enemy {name} died. Number: {enemyNumber}");

        if (animator != null && !string.IsNullOrEmpty(animDeath))
            animator.SetBool(animDeath, true);

        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.enabled = false;

        GameUIManager.Instance?.OnEnemyKilled();

        killer?.OnEnemyDefeatedSetPending(enemyNumber);

        if (rb != null) rb.velocity = Vector2.zero;

        Destroy(gameObject, 1f);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
