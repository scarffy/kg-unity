using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float changeInterval = 2f;
    
    [Header("Movement Method")]
    public MovementType movementType = MovementType.Rigidbody;
    
    [Header("Boundary Settings")]
    public bool useBoundaries = true;
    public Vector3 boundaryMin = new Vector3(-20, 0, -20);
    public Vector3 boundaryMax = new Vector3(20, 0, 20);
    
    [Header("Physics Settings")]
    public bool freezeY = true;
    public float drag = 0f;
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private float timer;
    private Vector3 initialPosition;

    public enum MovementType
    {
        Rigidbody,      // Physics-based (can collide)
        Transform,      // Direct transform movement (ignores physics)
        Kinematic       // Kinematic rigidbody (physics but no forces)
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        
        ConfigureRigidbody();
        
        ChooseRandomDirection();
        timer = changeInterval;
    }

    void ConfigureRigidbody()
    {
        if (rb == null) return;
        
        switch (movementType)
        {
            case MovementType.Rigidbody:
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.linearDamping = drag;
                if (freezeY) rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                break;
                
            case MovementType.Kinematic:
                rb.isKinematic = true;
                rb.useGravity = false;
                if (freezeY) rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                break;
                
            case MovementType.Transform:
                break;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ChooseRandomDirection();
            timer = changeInterval;
        }
        
        // Handle boundary checking
        if (useBoundaries)
        {
            CheckBoundaries();
        }
        
        // Transform-based movement
        if (movementType == MovementType.Transform)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // Physics-based movement
        if (rb != null && movementType != MovementType.Transform)
        {
            switch (movementType)
            {
                case MovementType.Rigidbody:
                    rb.linearVelocity = new Vector3(
                        moveDirection.x * moveSpeed,
                        rb.linearVelocity.y, // Preserve Y velocity if not frozen
                        moveDirection.z * moveSpeed
                    );
                    break;
                    
                case MovementType.Kinematic:
                    rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
                    break;
            }
        }
    }

    private void ChooseRandomDirection()
    {
        Vector3 newDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        if (newDirection.magnitude < 0.1f)
        {
            newDirection = Vector3.forward; // Default direction
        }
        
        moveDirection = newDirection;
        
        Debug.Log($"New direction: {moveDirection}, Speed: {moveSpeed}");
    }
    
    private void CheckBoundaries()
    {
        Vector3 pos = transform.position;
        bool hitBoundary = false;
        
        if (pos.x <= boundaryMin.x && moveDirection.x < 0)
        {
            moveDirection.x = Mathf.Abs(moveDirection.x); // Reverse X direction
            hitBoundary = true;
        }
        else if (pos.x >= boundaryMax.x && moveDirection.x > 0)
        {
            moveDirection.x = -Mathf.Abs(moveDirection.x); // Reverse X direction
            hitBoundary = true;
        }

        if (pos.z <= boundaryMin.z && moveDirection.z < 0)
        {
            moveDirection.z = Mathf.Abs(moveDirection.z); // Reverse Z direction
            hitBoundary = true;
        }
        else if (pos.z >= boundaryMax.z && moveDirection.z > 0)
        {
            moveDirection.z = -Mathf.Abs(moveDirection.z); // Reverse Z direction
            hitBoundary = true;
        }

        if (hitBoundary)
        {
            moveDirection = moveDirection.normalized;
            Debug.Log($"Hit boundary, new direction: {moveDirection}");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector3 normal = collision.contacts[0].normal;
            moveDirection = Vector3.Reflect(moveDirection, normal).normalized;
            
            moveDirection.y = 0;
            moveDirection = moveDirection.normalized;
            
            Debug.Log($"Collision reflection, new direction: {moveDirection}");
        }
    }
    
    // For debugging - show boundaries and direction
    void OnDrawGizmosSelected()
    {
        if (useBoundaries)
        {
            // Draw boundary box
            Gizmos.color = Color.yellow;
            Vector3 center = (boundaryMin + boundaryMax) * 0.5f;
            Vector3 size = boundaryMax - boundaryMin;
            Gizmos.DrawWireCube(center, size);
        }
        
        // Draw current direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, moveDirection * 3f);
        
        // Draw velocity if rigidbody exists
        if (rb != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, rb.linearVelocity);
        }
    }

    // Public method to get current velocity (for interception calculation)
    public Vector3 GetVelocity()
    {
        switch (movementType)
        {
            case MovementType.Rigidbody:
            case MovementType.Kinematic:
                return rb != null ? rb.linearVelocity : moveDirection * moveSpeed;
            case MovementType.Transform:
                return moveDirection * moveSpeed;
            default:
                return Vector3.zero;
        }
    }
}