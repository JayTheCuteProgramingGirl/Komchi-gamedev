using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour
{
    [Header("Attack-Player-System")]
    public int Damage = 10;
    bool Combo_Available;                                   //nicht in benutzung momentan
    //----//
    [Header("Attack-Buffers")]
    [SerializeField] private float AttackBufferTime = 2f;   //Attacke buffer (falls keine combo entsteht oder am ende einer combo)
    private float AttackBuffer;
    [SerializeField] private float ComboBufferTime = 2f;    //Wie viel zeit der spieler hat zwischen den ersten hit um zu entscheiden ob es eine Combo wird
    private float ComboBuffer;


    public float Range = 3f;
    private CircleCollider2D RangeComponent;
    private void Awake()
    {
        RangeComponent = GetComponent<CircleCollider2D>();

        RangeComponent.radius = Range;
    }

    private void Update()
    {
        //NICHT L�SCHEN WICHTIGE LOGIC F�R SPIELER ATTACKE!!--
        AttackBuffer -= Time.deltaTime;                                                  //L�sst den AttackBuffer runter z�hlen
        ComboBuffer -= Time.deltaTime;                                                   //L�sst den ComboBuffer runter z�hlen
        //NICHT L�SCHEN WICHTIGE LOGIC F�R SPIELER ATTACKE!!--
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Input.GetMouseButtonDown(0) && other.tag == "Enemy" && AttackBuffer < 0f)   //Checkt ob der Spieler Links-Click dr�ckt und das das in der "Attack-Zone" ein Gegner ist und der Buffer nicht greift
        {                                                                               //-----//
            AttackBuffer = AttackBufferTime;                                            //Aktiviert den AttackBuffer
            Single_Attack_Player(other);                                                //Ruft die Single_Attack_Player(); methode in der "Attack Logic" region auf und gibt dieser den getroffenen Gegner
        }                                                                               //-----//                                        
    }

    private void Single_Attack_Player(Collider2D Enemy)
    {
        Azim Enemy_Script = Enemy.GetComponent<Azim>();                                 //Nimmt von dem Gegener das script und speicher deis als Enemy_Scipt in dieser Methode
                                                                                        //----//
        Enemy_Script.Health -= Damage;                                                  //Zieht von dem Gegner Health Damage ab
                                                                                        //----//
        //Debug.Log(Enemy_Script.Health);                                               //Noch zu L�schen
    }
}
