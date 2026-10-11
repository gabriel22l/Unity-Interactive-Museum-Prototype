using UnityEngine;

public class PlayerMovementFP : MonoBehaviour, IMovementData
{
    private PlayerInput playerInput;
    private CharacterController characterController;
    private Camera cameraM;
    private Vector2 MoveInput => playerInput.MoveInput;
    private bool IsInputRunning => playerInput.IsRunning;
    
    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private float runMultiplier = 3;
    //[SerializeField] private float rotationSpeed = 10; //not used
    private readonly float accelerationRate = 50f;
    
    private readonly float gravity = -9.81f;
    
    private float currentVerticalVelocity;
    private Vector3 currentVelocityVector;
    
    public float CurrentVerticalVelocity => currentVerticalVelocity;
    public Vector3 CurrentVelocityVector => currentVelocityVector;
    
    public float Velocity {
        get
        {
            Vector3 v = characterController.velocity;
            v.y = 0;
            float mag = v.magnitude;
            return mag < 0.01f ? 0f : mag;
        }
    }
    public float MaxVelocity => IsInputRunning ? moveSpeed * runMultiplier : moveSpeed;
    public bool IsRunning => IsInputRunning;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        cameraM = Camera.main;
    }
    private void Update()
    {
        Move();
    }
    private void Move()
    {
        if (!characterController.enabled) return;
        Vector3 moveDir = GetMoveDirection();
        float targetSpeed = IsInputRunning ? moveSpeed * runMultiplier : moveSpeed;
        
        float directionalMultiplier = GetDirectionalSpeedMultiplier(moveDir);
        
        Vector3 targetVelocity = moveDir * targetSpeed * directionalMultiplier;
        currentVelocityVector = Vector3.MoveTowards(currentVelocityVector, targetVelocity, Time.deltaTime * accelerationRate);
        
        Rotate();
        
        Vector3 finalVelocity = currentVelocityVector;
        finalVelocity.y = GetVerticalVelocity();
        
        characterController.Move(Time.deltaTime * finalVelocity);
    }
    private Vector3 GetMoveDirection()
    {
        if(MoveInput.magnitude < 0.01f) return Vector3.zero;
        //get move vector in relation to camera 
        Vector3 cameraForward = cameraM.transform.forward;
        Vector3 cameraRight = cameraM.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        Vector3 moveDirection = (cameraForward * MoveInput.y) + (cameraRight * MoveInput.x);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1);
        return moveDirection;
    }
    private void Rotate()
    {
        Vector3 cameraForward = cameraM.transform.forward;
        cameraForward.y = 0;
        if (cameraForward.magnitude < 0.01f)
            return;
        transform.forward = cameraForward;
    }
    private float GetVerticalVelocity()
    {
        if (characterController.isGrounded && currentVerticalVelocity < 0)
        {
            currentVerticalVelocity = -2f;
        }
        else
        {
            currentVerticalVelocity += gravity * Time.deltaTime;
        }
        return currentVerticalVelocity;
    }
    private float GetDirectionalSpeedMultiplier(Vector3 direction)
    {
        //returns fractional multiplier if movement is sideways or backwards
        if (direction.magnitude < 0.01f) return 0;
        
        Vector3 cameraForward = cameraM.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        float dot =  Vector3.Dot(direction, cameraForward);
        float multiplier = Mathf.Lerp(0.6f, 1f, (dot + 1) / 2);
        return multiplier;
    }
    
    //external
    public void SetVelocity(Vector3 velocity, float verticalVelocity)
    {
        currentVelocityVector = velocity;
        currentVerticalVelocity = verticalVelocity;
    }
}