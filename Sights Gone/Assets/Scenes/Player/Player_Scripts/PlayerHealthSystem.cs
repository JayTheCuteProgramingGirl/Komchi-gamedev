using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    [Header("Health-Settings")]
    public static float MaxHealth = 100f; //Diese Variabel können bei jedem erdenklichen Script aufgerufen werden; 
    public static float currentHealth; 
    void Start()
    {
        currentHealth = MaxHealth; 
    }

    
    void Update()
    {
        Die(); 
    }

    private void Die()
    {
        if(currentHealth <= 0f) //Wenn zu wenig Leben
        {
            Destroy(gameObject); //Zerstöre Spieler; 
        }
    }
}
