using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Important References")]
    private Rigidbody2D PlayerRb;
    private Vector2 direction; 

    [HideInInspector] public float MovementDirection;

    [Header("Player Movement Settings")]
    [HideInInspector] public bool allowWalking = true;
    [SerializeField] private float PlayerWalkingSpeed;

    [Space(10f)]

    [Header("Player Gravity Settings")]
    public bool IsGrounded = true;
    [SerializeField] private LayerMask groundLayer; 
    [SerializeField] private Transform groundCheck; 
    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private float MaxGravityApplayer = 1f;
    private float gravityWaitTime;
    private bool gravityApplied = false; 

    [Header("Dash-Player-System")]
    [SerializeField] private float DashPower = 10f; 
    [SerializeField] private float DashCooldown; 
    public bool allowDash = true; 
    private Coroutine DashCoroutine;

    [Header("Jump_System")]
    [SerializeField] private float jumpForce = 5f; 

    void Awake()
    {
        PlayerRb = GetComponent<Rigidbody2D>();
        PlayerRb.gravityScale = 0f; 
        gravityWaitTime = MaxGravityApplayer;
    }

    void Update()
    {
        GetInputs(); // Bekomme spieler Inputs
        Clamps(); // Clamepen von werten!

        if (Input.GetMouseButtonDown(1) && Mathf.Abs(MovementDirection) > 0 && DashCoroutine == null && allowDash == true) //Wenn entwerde nach Links/Rechts, der Timer noch nichts getartet wurde und Recht-Klick gedrückt wird
        {
            StartCoroutine(DashSystem());
            Debug.Log("Der Dash wurde gestartet");
        }
    }

    void FixedUpdate()
    {
        CheckGrounded(); //Der Spieler kann nur rennen wenn erlaubt!

        if (allowWalking) 
        {
            PlayerMovementMethod();
        }

        if (!IsGrounded) // Gravitation wird nur benutzt wenn der Spieler nicht auf den Boden ist
        {
            Gravity(); 
        }
        else // Andernfalls ist die schwerkraft immer auf 0
        {
            PlayerRb.gravityScale = 0f; 
            gravityApplied = false; 
        }
    }

    #region PlayerMethods

    private void GetInputs()
    {
        MovementDirection = Input.GetAxisRaw("Horizontal"); 

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            Jump();
        }
    }

    private void Clamps()
    {
        gravityWaitTime = Mathf.Clamp(gravityWaitTime, 0f, MaxGravityApplayer);
    }

    private void PlayerMovementMethod()
    {
        Vector2 newVelocity = PlayerRb.velocity;
        newVelocity.x = MovementDirection * PlayerWalkingSpeed;
        PlayerRb.velocity = newVelocity;
    }

    private void Gravity() //Gravitation wird erst aktiviert wenn man zulange in der Luft ist, damit falls man springt nicht direkt die gravitation auf einen wirkt sondern erst wenn der Jump zu ende ist
    {
        if (!gravityApplied) 
        {
            gravityWaitTime -= 15f * Time.fixedDeltaTime;

            if (gravityWaitTime <= 0f)
            {
                PlayerRb.gravityScale = 1f; 
                gravityApplied = true;
                gravityWaitTime = MaxGravityApplayer; 
            }
        }
    }

    private void CheckGrounded() //ist der Spieler auf dem Boden mit Oberlap.. 
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer); 
    }

    private void Jump()
    {
        PlayerRb.velocity = new Vector2(PlayerRb.velocity.x, jumpForce); 
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(groundCheck.transform.position, groundCheckRadius);
    }


    #region Courutines

    private IEnumerator DashSystem()
    {
        if (Mathf.Abs(MovementDirection) < 0.1f)  // Keine Dash-Eingabe, wenn keine Bewegung
        {
            yield break;  // Frühzeitiger Abbruch der Coroutine
        }

        direction = new Vector2(MovementDirection, 0).normalized;

        // Deaktiviert das Gehen während des Dashes
        allowWalking = false;

        // Setzt sofort die Dash-Geschwindigkeit
        PlayerRb.velocity = direction * DashPower;

        // Wartet für den DashCooldown
        yield return new WaitForSeconds(DashCooldown);

        // Sanftes Stoppen des Spielers
        float lerpTime = 0.2f;
        while (lerpTime > 0f)
        {
            lerpTime -= Time.deltaTime;
            PlayerRb.velocity = Vector2.Lerp(PlayerRb.velocity, Vector2.zero, 0.2f);
            yield return null;
        }

        // Erlaubt das Gehen wieder
        allowWalking = true;

        DashCoroutine = null;
    }

    #endregion
}
