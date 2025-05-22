using UnityEngine;
using System.Linq; // Required for Count() if you prefer LINQ, otherwise manual loop is fine.

public class Weapon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectile;
    public float angleBetweenShots = 15f; // Angle in degrees between each shot in a spread

    public void Shoot()
    {
        AudioManager.Instance.PlaySound("PlayerShoot");

        int doubleShotItemCount = 0;
        // Ensure ItemManager and its gameState are available
        if (ItemManager.Instance != null && ItemManager.Instance.gameState != null && ItemManager.Instance.gameState.activeWeaponModifiers != null)
        {
            foreach (var modifier in ItemManager.Instance.gameState.activeWeaponModifiers)
            {
                if (modifier.modifierType == WeaponModifierType.DoubleShot)
                {
                    doubleShotItemCount++;
                }
            }
        }

        int numberOfBullets = 1 + doubleShotItemCount;

        if (numberOfBullets == 1)
        {
            // Fire a single bullet straight ahead
            Instantiate(projectile, firePoint.position, firePoint.rotation);
        }
        else
        {
            // Calculate the starting angle for the arc
            // The total spread will be (numberOfBullets - 1) * angleBetweenShots
            // The first shot will be at -totalSpread / 2 from the center
            float totalSpreadArc = (numberOfBullets - 1) * angleBetweenShots;
            float startAngleOffset = -totalSpreadArc / 2f;

            for (int i = 0; i < numberOfBullets; i++)
            {
                // Calculate the angle for the current bullet
                float currentAngle = startAngleOffset + (i * angleBetweenShots);

                // Apply this angle offset to the firePoint's current rotation
                Quaternion shotRotation = firePoint.rotation * Quaternion.Euler(0, 0, currentAngle);

                Instantiate(projectile, firePoint.position, shotRotation);
            }
        }
    }
}