using UnityEngine;

public class Weapon : MonoBehaviour
{
     public Transform firePoint;
     public GameObject projectile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {

    }

    public void Shoot() {
        Instantiate(projectile, firePoint.position, firePoint.rotation);
    }

}
