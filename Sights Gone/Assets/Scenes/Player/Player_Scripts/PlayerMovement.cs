using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [Header("Important References")]
    private Rigidbody2D PlayerRb;
    private Vector2 direction; 

    [HideInInspector] public static float MovementDirection;

    [Header("Player Movement Settings")]
    [HideInInspector] public static bool allowWalking = true;
    [SerializeField] private float PlayerWalkingSpeed;

    [Space(10f)]

    [Header("Player Gravity Settings")]
    public static bool IsGrounded = true;
    [SerializeField] private LayerMask groundLayer; 
    [SerializeField] private Transform groundCheck; 
    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private float MaxGravityApplayer = 1f;
    private float gravityWaitTime;
    private bool gravityApplied = false;

    [Header("Input Buffers")]
    [SerializeField] private float InputBufferTime = 2f;
    private float InputBuffer;

    [Header("Dash-Player-System")]
    [SerializeField] private float DashPower = 10f; 
    [SerializeField] private float DashCooldown;

    public bool allowDash = true; 
    [HideInInspector] public static bool iscurrentlyDashing = false; // Ist der Spieler gerade am Dashen
    private Coroutine DashCoroutine;

    [Header("Jump_System")]
    [SerializeField] private float jumpForce = 5f; 
    [SerializeField] private float maxHoldTime = 0.1f; // Maximale Zeit, in der die Sprunghöhe erhöht wird
    [SerializeField] private float maxJumpHeight = 6f; //Maximale Sprung höhe; 
    [SerializeField] private float gravityJumpMultiplierer = 2f; //Die Gravitation wir erhört wenn der Spieler runter fällt nach einem Sprung


    private bool isJumping = false; 
    private float JumpTimeCounter = 1.0f; //Der Timer der zählt wie lange der Spieler schon auf den Boden ist
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
        Jump(); //Jump wird genutzt; 
 
        if (Input.GetMouseButtonDown(1) && Mathf.Abs(MovementDirection) > 0 && DashCoroutine == null && allowDash == true && InputBuffer < 0f) //Wenn entwerde nach Links/Rechts, der Timer noch nichts getartet wurde und Recht-Klick gedrückt wird und der InputBuffer "Cooldown" hattte
        {
            InputBuffer = InputBufferTime;
            StartCoroutine(DashSystem());

        }
        else { InputBuffer -= Time.deltaTime; }
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
    }

    private void Jump() //Methode fürs Springen, in Update!
    {
        if (Input.GetButtonDown("Jump") && IsGrounded == true) // Wenn die Sprungtaste gedrückt wird, und der Spieler auf den Boden ist
        {
            PlayerRb.velocity = new Vector2(PlayerRb.velocity.x, jumpForce);
            isJumping = true; // Der Spieler springt
            JumpTimeCounter = maxHoldTime; // Timer zurücksetzen
        }

        if (Input.GetButton("Jump") && isJumping == true) // Wenn die Sprungtaste gehalten wird und der Spieler noch springen kann
        {
            if (JumpTimeCounter > 0)
            {

                float currentYVelocity = PlayerRb.velocity.y;
                float newYVelocity = Mathf.Lerp(currentYVelocity, maxJumpHeight, 0.1f); // 0.1f = Interpolationsrate, kann erhört werden wenn du willst jay, kannst gerne dazu eine vaiabel erstellen
                PlayerRb.velocity = new Vector2(PlayerRb.velocity.x, newYVelocity);

                JumpTimeCounter -= Time.deltaTime; // Zeit runterzählen
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetButtonUp("Jump")) // Wenn die Sprungtaste losgelassen wird
        {
            isJumping = false; // Spieler stoppt der Sprung
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

        if (PlayerRb.velocity.y < 0f) // Wenn der Spieler nach unten fällt
        {
            PlayerRb.gravityScale = Mathf.Lerp(PlayerRb.gravityScale, gravityJumpMultiplierer, 0.1f); // Sanfte Erhöhung der Schwerkraft
        }

  
    }

    private void CheckGrounded() //ist der Spieler auf dem Boden mit Oberlap.. 
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer); 
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(groundCheck.transform.position, groundCheckRadius);
    }


    #region Corutines

    private IEnumerator DashSystem()
    {
        if (Mathf.Abs(MovementDirection) < 0.1f)  // Keine Dash-Eingabe, wenn keine Bewegung
        {
            yield break;  // Frühzeitiger Abbruch der Coroutine
        }
        iscurrentlyDashing = true; //Der Spieler Dasht gerade
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
        iscurrentlyDashing = false;  //Der Spieler Dasht nicht mehr
        DashCoroutine = null; //Courutine Referenz zurückk!
    }

    #endregion
}