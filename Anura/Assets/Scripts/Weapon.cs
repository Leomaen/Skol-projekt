using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        
    }

    void Shoot() {
        // Instantiate(projectile, firePoint.rotation);
    }

}
