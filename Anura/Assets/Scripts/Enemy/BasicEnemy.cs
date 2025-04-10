using UnityEngine;

public class BasicEnemy : EnemyBase
{
    [SerializeField] private float chargeSpeed = 4.0f;
    [SerializeField] private float chargeInterval = 5.0f;
    [SerializeField] private float chargeDuration = 1.0f;
    
    private float lastChargeTime;
    private bool isCharging = false;
    private Vector2 chargeDirection;

    protected override void Start()
    {
        base.Start();
        lastChargeTime = -chargeInterval; // Allow charging soon after spawn
    }

    protected override void Update()
    {
        base.Update();
        
        // Handle charging behavior
        if (isChasing && !isCharging && Time.time > lastChargeTime + chargeInterval)
        {
            StartCharging();
        }
        
        if (isCharging && Time.time > lastChargeTime + chargeDuration)
        {
            StopCharging();
        }
    }

    protected override void FixedUpdate()
    {
        if (isCharging)
        {
            // Move faster in the charge direction
            rb.linearVelocity = chargeDirection * chargeSpeed;
        }
        else
        {
            base.FixedUpdate();
        }
    }

    protected override void ChooseNewPath()
    {
        base.ChooseNewPath();
        
        // BasicEnemy moves a bit more erratically
        float additionalRandomness = Random.Range(-45f, 45f);
        currentMoveDirection = RotateVector(currentMoveDirection, additionalRandomness);
    }
    
    private void StartCharging()
    {
        if (playerTransform == null) return;
        
        isCharging = true;
        lastChargeTime = Time.time;
        chargeDirection = (playerTransform.position - transform.position).normalized;
        
        // Play charge animation or sound
        if (animator != null)
        {
            animator.SetTrigger("Charge");
        }
    }
    
    private void StopCharging()
    {
        isCharging = false;
    }
    
    // Helper method to rotate a vector by angle in degrees
    private Vector2 RotateVector(Vector2 v, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);
        
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}
