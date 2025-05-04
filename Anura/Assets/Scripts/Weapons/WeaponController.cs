using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    public void Shoot()
    {
        // Check for weapon modifiers
        if (ItemManager.Instance != null)
        {
            bool hasDoubleShot = false;
            bool hasSpread = false;
            
            foreach (WeaponModifier modifier in ItemManager.Instance.activeWeaponModifiers)
            {
                if (modifier.modifierType == WeaponModifierType.DoubleShot)
                    hasDoubleShot = true;
                else if (modifier.modifierType == WeaponModifierType.Spread)
                    hasSpread = true;
            }
            
            if (hasDoubleShot)
            {
                ShootDoubleShot();
            }
            else if (hasSpread)
            {
                ShootSpread();
            }
            else
            {
                ShootNormal();
            }
        }
        else
        {
            ShootNormal();
        }
    }
    
    void ShootNormal()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
    
    void ShootDoubleShot()
    {
        // Offset positions for double shot
        Vector3 offset1 = firePoint.TransformPoint(new Vector3(0, 0.15f, 0)) - firePoint.position;
        Vector3 offset2 = firePoint.TransformPoint(new Vector3(0, -0.15f, 0)) - firePoint.position;
        
        Instantiate(bulletPrefab, firePoint.position + offset1, firePoint.rotation);
        Instantiate(bulletPrefab, firePoint.position + offset2, firePoint.rotation);
    }
    
    void ShootSpread()
    {
        // Center shot
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Side shots at angles
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, 15));
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, -15));
    }
}
