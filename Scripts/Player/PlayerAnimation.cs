using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private int speedHash;
    private int deadHash;

    private IMovementData iMovementData;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        speedHash = Animator.StringToHash("Speed");
        deadHash = Animator.StringToHash("Dead");
        if (iMovementData == null)
            iMovementData = GetComponentInParent<IMovementData>(); //fallback, gets overriden on PlayerViewController
    }
    private void Update()
    {
        AnimateMovement();
    }
    private void AnimateMovement()
    {
        if (iMovementData == null)
            return;
        float maxValue = iMovementData.IsRunning ? 1 : 0.5f;
        float speed = Mathf.Clamp01(iMovementData.Velocity / iMovementData.MaxVelocity);
        speed *= maxValue;
        animator.SetFloat(speedHash, speed, 0.1f, Time.deltaTime);
        //Debug.Log($"IMovementData.Velocity: {iMovementData.Velocity}, maxValue: {maxValue}, speed: {speed}");
    }
    public void SetMovementData(IMovementData movementData)
    {
        iMovementData = movementData;
    }
    public void PlayDeath()
    {
        animator.SetBool(deadHash, true);
    }
}