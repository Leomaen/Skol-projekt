using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectile;


    public void Shoot()
    {
        AudioManager.Instance.PlaySound("PlayerShoot");
        Instantiate(projectile, firePoint.position, firePoint.rotation);
    }

}
