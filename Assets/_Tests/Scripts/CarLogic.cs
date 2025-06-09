using UnityEngine;
using TMPro;

public class CarLogic : MonoBehaviour
{
    public CarEngineState carEngineState;
    
    [Header("Car Settings")]
    public float forwardAcceleration = 2f;  // m/s²
    public float brakeDeceleration = 3f;    // m/s²
    public float maxSpeed = 10f;            // m/s
    
    [Header("Target")]
    public Transform target;               
    
    [Header("Current State - Read Only")]
    public float currentSpeed = 0f;
    public float distanceToTarget = 0f;
    public EngineState currentEngineState;
    
    [Header("UI Elements")]
    public TMP_Text currentSpeedText;
    
    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned to CarLogic");
        }
    }
    
    void Update()
    {
        if(target == null)
            return;
        
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        currentEngineState = carEngineState.DetermineEngineState(distanceToTarget, currentSpeed, 
            forwardAcceleration, brakeDeceleration, maxSpeed);
        
        ApplyEngineState(currentEngineState);
        
        MoveCar();
        
        UpdateDebugInfo();
    }
    
    void ApplyEngineState(EngineState state)
    {
        switch (state)
        {
            case EngineState.Accelerate:
                currentSpeed += forwardAcceleration * Time.deltaTime;
                break;
                
            case EngineState.Brake:
                currentSpeed -= brakeDeceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(0f, currentSpeed);
                break;
                
            case EngineState.Idle:
                break;
        }
    }
    
    void MoveCar()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;
    }
    
    void UpdateDebugInfo()
    {
        currentSpeedText.text = $"Distance: {distanceToTarget:F2}m | Speed: {currentSpeed:F2}m/s | State: {currentEngineState}";
    }
}
