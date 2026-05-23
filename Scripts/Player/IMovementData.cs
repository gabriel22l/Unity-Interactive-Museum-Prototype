using UnityEngine;

public interface IMovementData
{
    float Velocity { get; }
    float MaxVelocity { get; }
    bool IsRunning { get; }
}
