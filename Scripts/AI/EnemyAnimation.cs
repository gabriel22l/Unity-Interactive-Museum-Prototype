using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private EnemyAIController aiController;
    private Animator animator;
    private int speedHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        speedHash = Animator.StringToHash("Speed");
    }

    private void Update()
    {
        float maxValue = aiController.IsRunning ? 1 : 0.5f;
        float processedSpeed = Mathf.Clamp01(aiController.CurrentSpeed / aiController.CurrentMaxSpeed); //0 to 1
        processedSpeed *= maxValue;
        animator.SetFloat(speedHash, processedSpeed, 0.1f, Time.deltaTime);
    }
}
