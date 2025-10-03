using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy data")]
    public long enemyNumber = 1;
    public long maxHealth = 10;
    long currentHealth;

    [Header("Feedback")]
    public GameObject deathVfx;
    public AudioClip deathSfx;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(long damage, PlayerController attacker = null)
    {
        if (damage <= 0) return;
        currentHealth -= damage;
        Debug.Log($"{name} took {damage} damage. Remaining HP: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die(attacker);
        }
    }

    void Die(PlayerController killer)
    {
        if (killer != null)
        {
            killer.OnEnemyDefeatedSetPending(enemyNumber);
        }

        if (deathVfx != null) Instantiate(deathVfx, transform.position, Quaternion.identity);
        if (deathSfx != null && SoundManager.InstanceExists) SoundManager.Instance.PlaySFX(deathSfx);

        Destroy(gameObject);
    }
}
