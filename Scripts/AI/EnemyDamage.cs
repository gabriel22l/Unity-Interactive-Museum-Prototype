using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float damageAmount = 35f;
    
    public bool CanDamage { get; set; } = true;
    private void OnTriggerStay(Collider other)
    {
        if(!CanDamage)  return;
        if (!other.TryGetComponent(out Health health))
            return;
        health.TakeDamage(damageAmount);
    }
}
