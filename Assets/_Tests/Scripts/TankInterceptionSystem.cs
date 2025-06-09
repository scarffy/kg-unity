using UnityEngine;

public class TankInterceptionSystem : MonoBehaviour
{
    [Header("Tank Settings")]
    public Transform tankTurret;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 50f;
    
    [Header("Target")]
    public Transform target;
    public Rigidbody targetRigidbody;
    
    [Header("Debug")]
    public bool showDebugLines = true;
    public bool showInterceptPoint = true;
    public GameObject interceptPointVisualizer;
    
    private Vector3 lastInterceptPosition;
    private bool canIntercept = false;

    void Update()
    {
        if (target != null)
        {
            Vector3 interceptPos;
            bool possible = CalculateInterceptPosition(
                transform.position,
                GetComponent<Rigidbody>().linearVelocity,
                target.position,
                targetRigidbody.linearVelocity,
                bulletSpeed,
                out interceptPos
            );

            if (possible)
            {
                lastInterceptPosition = interceptPos;
                canIntercept = true;
                
                AimTurretAtPosition(interceptPos);
                
                if (interceptPointVisualizer != null)
                {
                    interceptPointVisualizer.transform.position = interceptPos;
                    interceptPointVisualizer.SetActive(showInterceptPoint);
                }
            }
            else
            {
                canIntercept = false;
                if (interceptPointVisualizer != null)
                {
                    interceptPointVisualizer.SetActive(false);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && canIntercept)
        {
            Fire();
        }
    }

    /// <summary>
    /// The main interception calculation method
    /// </summary>
    bool CalculateInterceptPosition(Vector3 selfPosition, Vector3 selfVelocity, 
                                   Vector3 targetPosition, Vector3 targetVelocity, 
                                   float bulletSpeed, out Vector3 interceptPosition)
    {
        interceptPosition = Vector3.zero;
        
        Vector3 relativePosition = targetPosition - selfPosition;
        Vector3 relativeVelocity = targetVelocity - selfVelocity;
        
        float a = Vector3.Dot(relativeVelocity, relativeVelocity) - (bulletSpeed * bulletSpeed);
        float b = 2 * Vector3.Dot(relativePosition, relativeVelocity);
        float c = Vector3.Dot(relativePosition, relativePosition);
        
        float discriminant = b * b - 4 * a * c;
        
        if (discriminant < 0)
        {
            Debug.Log("Target moving too fast - cannot intercept");
            return false;
        }
        
        if (Mathf.Abs(a) < 1e-6f)
        {
            if (Mathf.Abs(b) < 1e-6f)
            {
                Debug.Log("No solution - parallel motion");
                return false;
            }
            float t = -c / b;
            if (t >= 0)
            {
                interceptPosition = targetPosition + targetVelocity * t;
                return true;
            }
            return false;
        }
        
        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtDiscriminant) / (2 * a);
        float t2 = (-b + sqrtDiscriminant) / (2 * a);
        
        float timeToIntercept = -1;
        
        if (t1 >= 0 && t2 >= 0)
        {
            timeToIntercept = Mathf.Min(t1, t2);
        }
        else if (t1 >= 0)
        {
            timeToIntercept = t1;
        }
        else if (t2 >= 0)
        {
            timeToIntercept = t2;
        }
        
        if (timeToIntercept < 0)
        {
            Debug.Log("No positive solution - cannot intercept");
            return false;
        }
        
        interceptPosition = targetPosition + targetVelocity * timeToIntercept;
        
        return true;
    }

    void AimTurretAtPosition(Vector3 targetPos)
    {
        if (tankTurret == null) return;
        
        Vector3 directionToTarget = (targetPos - tankTurret.position).normalized;
        
        float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        
        float currentAngle = tankTurret.eulerAngles.y;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 5f);
        
        tankTurret.rotation = Quaternion.Euler(0, newAngle, 0);
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            Vector3 fireDirection = (lastInterceptPosition - firePoint.position).normalized;
            bulletRb.linearVelocity = fireDirection * bulletSpeed;
        }
        
        bullet.AddComponent<BulletBehavior>();
        
        Debug.Log($"Fired at intercept position: {lastInterceptPosition}");
    }

    void OnDrawGizmos()
    {
        if (!showDebugLines) return;
        
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
        
        if (canIntercept)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, lastInterceptPosition);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastInterceptPosition, 0.5f);
        }
        
        if (target != null && targetRigidbody != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(target.position, targetRigidbody.linearVelocity);
        }
    }
}