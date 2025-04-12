using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    [Header("Combat Stats")]
    public int damage;
    public int bulletSpeed;
    public float atkSpeed;

    [Header("Health Stats")]
    public int PlayerHealth;
    public int maxHealth;
    
    [Header("Movement Stats")]
    public float movementSpeed;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

}
