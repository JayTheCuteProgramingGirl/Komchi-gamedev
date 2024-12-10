using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    #region Variabeln
    [Header("Important References")]
    private Rigidbody2D PlayerRb;
    private Vector2 direction;
    //----//
    [HideInInspector] public static float MovementDirection;
    //----//
    [Header("Player Movement Settings")]
    [HideInInspector] public static bool allowWalking = true;
    [SerializeField] private float PlayerWalkingSpeed;
    //----//
    [Space(10f)]
    //----//
    [Header("Player Gravity Settings")]
    public static bool IsGrounded = true;
    [SerializeField] private LayerMask groundLayer; 
    [SerializeField] private Transform groundCheck; 
    [SerializeField] private float groundCheckRadius = 0.2f;
    //----//
    [SerializeField] private float MaxGravityApplayer = 1f;
    private float gravityWaitTime;
    private bool gravityApplied = false;
    //----//
    [Header("Input Buffers")]
    [SerializeField] private float InputBufferTime = 2f;    //Input buffer für den Dash
    private float InputBuffer;
    [SerializeField] private float AttackBufferTime = 2f;   //Attacke buffer (falls keine combo entsteht oder am ende einer combo)
    private float AttackBuffer;
    [SerializeField] private float ComboBufferTime = 2f;    //Wie viel zeit der spieler hat zwischen den ersten hit um zu entscheiden ob es eine Combo wird
    private float ComboBuffer;
    //----//
    [Header("Dash-Player-System")]
    [SerializeField] private float DashPower = 10f; 
    [SerializeField] private float DashDuration;
    //----//
    public bool allowDash = true; 
    [HideInInspector] public static bool iscurrentlyDashing = false; // Ist der Spieler gerade am Dashen
    private Coroutine DashCoroutine;
    //----//
    [Header("Jump_System")]
    [SerializeField] private float jumpForce = 5f; 
    [SerializeField] private float maxHoldTime = 0.1f; // Maximale Zeit, in der die Sprunghöhe erhöht wird
    [SerializeField] private float maxJumpHeight = 6f; //Maximale Sprung höhe; 
    [SerializeField] private float gravityJumpMultiplierer = 2f; //Die Gravitation wir erhört wenn der Spieler runter fällt nach einem Sprung
    //----//
    [Header("Attack-Player-System")]
    public int Damage;
    bool Combo_Available; //nicht in benutzung momentan
    //----//
    private bool isJumping = false; 
    private float JumpTimeCounter = 1.0f; //Der Timer der zählt wie lange der Spieler schon auf den Boden ist

    #endregion

    #region Awake
    void Awake()
    {
        PlayerRb = GetComponent<Rigidbody2D>();
        PlayerRb.gravityScale = 0f; 
        gravityWaitTime = MaxGravityApplayer;
    }
    #endregion

    #region Update Methode
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


        //NICHT LÖSCHEN WICHTIGE LOGIC FÜR SPIELER ATTACKE!!--
        AttackBuffer -= Time.deltaTime;                                                  //Lässt den AttackBuffer runter zählen
        ComboBuffer -= Time.deltaTime;                                                   //Lässt den ComboBuffer runter zählen
        //NICHT LÖSCHEN WICHTIGE LOGIC FÜR SPIELER ATTACKE!!--
    }
    #endregion

    private void OnTriggerStay2D(Collider2D other)
    {
        if(Input.GetMouseButtonDown(0) &&  other.tag == "Enemy" && AttackBuffer < 0f)   //Checkt ob der Spieler Links-Click drückt und das das in der "Attack-Zone" ein Gegner ist und der Buffer nicht greift
        {                                                                               //-----//
            AttackBuffer = AttackBufferTime;                                            //Aktiviert den AttackBuffer
            Single_Attack_Player(other);                                                //Ruft die Single_Attack_Player(); methode in der "Attack Logic" region auf und gibt dieser den getroffenen Gegner
        }                                                                               //-----//                                        
    }

    #region Fixed Update
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
    #endregion

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
        yield return new WaitForSeconds(DashDuration);

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

    #region Attack Logic

    private void Single_Attack_Player(Collider2D Enemy)
    {
        Azim Enemy_Script = Enemy.GetComponent<Azim>();                                 //Nimmt von dem Gegener das script und speicher deis als Enemy_Scipt in dieser Methode
                                                                                        //----//
        Enemy_Script.Health -= Damage;                                                  //Zieht von dem Gegner Health Damage ab
                                                                                        //----//
        //Debug.Log(Enemy_Script.Health);                                               //Noch zu Löschen
    }




    #endregion
}