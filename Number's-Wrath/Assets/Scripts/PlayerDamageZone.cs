using UnityEngine;

public class PlayerDamageZone : MonoBehaviour
{
    [Tooltip("Punto al que se teletransporta el jugador al recibir daño.")]
    public Transform spawnPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ReceiveDamage(1);

            if (spawnPoint != null)
                player.transform.position = spawnPoint.position;
        }
    }
}
