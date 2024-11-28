using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Important References")]
    private Rigidbody2D PlayerRb;

    // Bewegungsrichtung
    [HideInInspector] public float MovementDirection;

    [Header("Player Movement Settings")]
    [HideInInspector] public bool allowWalking = true;
    [SerializeField] private float PlayerWalkingSpeed;

    [Space(10f)]

    [Header("Player Gravity Settings")]
    public bool IsGrounded = true;
    [SerializeField] private LayerMask groundLayer; // Layer für den Boden
    [SerializeField] private Transform groundCheck; // Position, wo der Boden überprüft wird
    [SerializeField] private float groundCheckRadius = 0.2f; // Radius für den Bodentest

    // Timer für die Gravitation
    [SerializeField] private float MaxGravityApplayer = 1f;
    private float ApplyGravityAfter;
    private bool gravityApplied = false; // Steuert, ob Gravitation bereits angewendet wurde

    [SerializeField] private float jumpForce = 5f; // Sprungkraft

    void Awake()
    {
        PlayerRb = GetComponent<Rigidbody2D>();
        PlayerRb.gravityScale = 0f; // Keine Gravitation zu Beginn
        ApplyGravityAfter = MaxGravityApplayer;
    }

    void Update()
    {
        GetInputs(); // Eingaben abfragen
        Clamps();
    }

    void FixedUpdate()
    {
        CheckGrounded(); // Überprüfen, ob der Spieler auf dem Boden steht

        if (allowWalking) // Bewegung erlauben
        {
            PlayerMovementMethod();
        }

        if (!IsGrounded) // Spieler ist in der Luft
        {
            Gravity(); // Gravitation nach Verzögerung anwenden
        }
        else // Spieler ist am Boden
        {
            PlayerRb.gravityScale = 0f; // Gravitation deaktivieren
            gravityApplied = false; // Gravitation kann neu ausgelöst werden
        }
    }

    #region PlayerMethods

    private void GetInputs()
    {
        MovementDirection = Input.GetAxis("Horizontal");

        // Sprung auslösen
        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            Jump();
        }
    }

    private void Clamps()
    {
        ApplyGravityAfter = Mathf.Clamp(ApplyGravityAfter, 0f, MaxGravityApplayer);
    }

    private void PlayerMovementMethod()
    {
        Vector2 newVelocity = PlayerRb.velocity;
        newVelocity.x = MovementDirection * PlayerWalkingSpeed;
        PlayerRb.velocity = newVelocity;
    }

    private void Gravity()
    {
        if (!gravityApplied) // Gravitation nur anwenden, wenn sie noch nicht aktiv ist
        {
            ApplyGravityAfter -= 15f * Time.fixedDeltaTime;

            if (ApplyGravityAfter <= 0f)
            {
                PlayerRb.gravityScale = 1f; // Gravitation aktivieren
                gravityApplied = true;
                ApplyGravityAfter = MaxGravityApplayer; // Timer zurücksetzen
                Debug.Log("Gravitation angewendet!");
            }
        }
    }

    private void CheckGrounded()
    {
        // Überprüft, ob der Spieler den Boden berührt
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Jump()
    {
        PlayerRb.velocity = new Vector2(PlayerRb.velocity.x, jumpForce); // Nach oben springen
        Debug.Log("Sprung ausgeführt!");
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(groundCheck.transform.position, groundCheckRadius);
    }
}
