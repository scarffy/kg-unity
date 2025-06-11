using UnityEngine;

public enum EngineState
{
    /// <summary>
    /// On returning this, currentSpeed += forwardAcceleration
    /// </summary>
    Accelerate,

    /// <summary>
    /// On returning this, the vehicle maintains currentSpeed.
    /// </summary>
    Idle,

    /// <summary>
    /// On returning this, currentSpeed -= brakeDeceleration. (Up to currentSpeed == 0)
    /// </summary>
    Brake,
}

public class CarEngineState : MonoBehaviour
{
    [Header("Debug - Current State")]
    public EngineState engineState = EngineState.Brake;
    
    public EngineState DetermineEngineState(float distanceToTarget, float currentSpeed, float forwardAcceleration,
        float brakeDeceleration, float maxSpeed)
    {
        EngineState result;
        
        if (distanceToTarget <= 0f)
        {
            result = currentSpeed > 0f ? EngineState.Brake : EngineState.Idle;
        }
        else if (currentSpeed <= 0f)
        {
            result = EngineState.Accelerate;
        }
        else
        {
            // Calculate stopping distance using: d = v² / (2a)
            float stoppingDistance = (currentSpeed * currentSpeed) / (2f * brakeDeceleration);
            
            if (distanceToTarget <= stoppingDistance)
            {
                result = EngineState.Brake;
            }
            else if (currentSpeed >= maxSpeed)
            {
                result = EngineState.Idle;
            }
            else
            {
                float nextSpeed = currentSpeed + forwardAcceleration;
                float nextStoppingDistance = (nextSpeed * nextSpeed) / (2f * brakeDeceleration);
                
                if (distanceToTarget - 1f <= nextStoppingDistance)
                {
                    result = EngineState.Idle;
                }
                else
                {
                    result = EngineState.Accelerate;
                }
            }
        }
        
        engineState = result;
        return result;
    }
}
