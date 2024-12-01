using UnityEngine;
using System.Collections;

public class CollisionHandler : MonoBehaviour
{
    [Header("Important-References")]
    private Rigidbody2D playerRB;
    private Azim spiderEnemyScriptRef;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f; // Die Stärke des Knockbacks
    [SerializeField] private float knockbackDuration = 0.2f; // Wie lange der Knockback dauert
    private bool isKnockedBack = false;

    private void Start()
    {
        playerRB = GetComponent<Rigidbody2D>(); // Holen des Rigidbody des Spielers
        spiderEnemyScriptRef = FindObjectOfType<Azim>();  // Finde das Azim-Skript (Spinne)
    }

    private void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.CompareTag("EnemySpider") && spiderEnemyScriptRef != null)
        {
            // Deaktiviere die Bewegung des Spielers während des Knockbacks
            PlayerMovement.allowWalking = false;

            // Bestimme die Richtung des Spielers relativ zur Spinne
            Vector2 knockbackDirection = GetKnockbackDirection();

            // Führe den Knockback aus
            ApplyKnockback(knockbackDirection);
        }
    }

    private Vector2 GetKnockbackDirection()
    {
        // Vertauschte Logik für die Knockback-Richtung
        // Wenn der Spieler links von der Spinne ist, wird er nach rechts gestoßen
        // Wenn der Spieler rechts von der Spinne ist, wird er nach links gestoßen
        if (spiderEnemyScriptRef.transform.position.x > transform.position.x)
        {
            return Vector2.left; // Spieler ist links von der Spinne, also nach rechts stoßen
        }
        else
        {
            return Vector2.right; // Spieler ist rechts von der Spinne, also nach links stoßen
        }
    }

    private void ApplyKnockback(Vector2 direction)
    {
        if (!isKnockedBack) // Verhindert mehrfachen Knockback während der Dauer
        {
            isKnockedBack = true;

            // Wende den Knockback an
            playerRB.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            PlayerHealthSystem.currentHealth -= Azim.NormalAttackDamage; //Reduzier Leben vom Spieler

            // Setze den Spieler nach der Knockback-Dauer zurück
            StartCoroutine(ResetKnockback());
        }
    }

    private IEnumerator ResetKnockback()
    {
        // Warte die Knockback-Dauer
        yield return new WaitForSeconds(knockbackDuration);

        // Setze den Knockback-Status zurück
        isKnockedBack = false;

        // Reaktiviere die Bewegung des Spielers nach der Knockback-Dauer
        PlayerMovement.allowWalking = true;
    }
}
