using UnityEngine;

public class LimitTrigger : MonoBehaviour
{
    [Header("Spawn Point")]
    public Transform spawnPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (player.CurrentLives > 1)
        {
            player.ReceiveDamage(1);

            other.transform.position = spawnPoint.position;

            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = Vector2.zero;
        }
        else
        {
            player.ReceiveDamage(1);
        }
    }
}
