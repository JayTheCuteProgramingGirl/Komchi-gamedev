using UnityEngine;

public class Azim : MonoBehaviour
{
    [Header("Important-References")]
    private Rigidbody2D enemyRb;
    public GameObject playerPosition;
    private Vector2 direction;

    [Header("Enemy-Settings")]
    [SerializeField] private float distanceToPlayer; 
    [SerializeField] private float jumpBackPower = 5f; 
    [SerializeField] private float minimumPlayerDistance;
    [SerializeField] private float enemyMoveSpeed = 3f; 
    [SerializeField] public static float NormalAttackDamage = 25f;
    [SerializeField] private int Damage = 50;
    public float timerForFollowPlayer = 2f; 
    private bool hasJumpedBack = false;
    //----!Temporär!----\\
    public int Health = 50;
    //----!Temporär!----\\
    void Awake()
    {
        enemyRb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        distanceToPlayer = Vector2.Distance(transform.position, playerPosition.transform.position); // Berechnung des Abstands
    }

    void FixedUpdate()
    {
        HandleJumpBack();
        FollowPlayer();
        if(Health <= 0 ) { Destroy(this.gameObject); }
    }

    private void HandleJumpBack()
    {

        if (distanceToPlayer < minimumPlayerDistance && !hasJumpedBack)
        {
            direction = (transform.position.x > playerPosition.transform.position.x) ? Vector2.right : Vector2.left;

            enemyRb.AddForce(direction * jumpBackPower, ForceMode2D.Impulse); // Gegner springt zurück

            hasJumpedBack = true;

            PlayerHealthSystem.currentHealth -= Damage;

            //PlayerMovement.allowWalking = false;

            //Vector2 knockbackDirection = CollisionHandler.GetKnockbackDirection();

            //CollisionHandler.ApplyKnockback(knockbackDirection);

        }
    }

    private void FollowPlayer()
    {
        if (hasJumpedBack)
        {
            timerForFollowPlayer -= Time.fixedDeltaTime; // Timer reduziert sich

            if (timerForFollowPlayer <= 0f)
            {
                // Berechne die Richtung zum Spieler
                direction = (playerPosition.transform.position - transform.position).normalized;

                // Bewege den Gegner in Richtung des Spielers
                enemyRb.MovePosition((Vector2)transform.position + direction * enemyMoveSpeed * Time.fixedDeltaTime);
            }
        }
    }

    // Methode, um die Richtung des Spielers zu bekommen
    public string GetPlayerDirection()
    {
        return (transform.position.x > playerPosition.transform.position.x) ? "left" : "right";
    }
}
