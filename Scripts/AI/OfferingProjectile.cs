using UnityEngine;

public class OfferingProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    private ItemWorldObject itemWorldObject;
    
    private bool hasBeenThrown = false;
    public bool Recoverable { get; set; } = true;
    public void Initialize(float force, Vector3 direction)
    {
        if (rb == null && !TryGetComponent(out rb)) return;
        rb.AddForce(direction * force, ForceMode.Impulse); 
        if(TryGetComponent(out itemWorldObject))
            itemWorldObject.SetInteractable(false);
        
        hasBeenThrown = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (collision.gameObject.TryGetComponent(out EnemyAIController enemyAIController))
        {
            enemyAIController.OnOfferingHit();
            Destroy(gameObject);
            return;
        }
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.TryGetComponent(out OfferingProjectile p))
        {
            if (!Recoverable)
            {
                Destroy(gameObject);
                return;
            }
            itemWorldObject?.SetInteractable(true);
            hasBeenThrown = false;
        }
    }
}
