using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private bool isInvulnerable;
    [SerializeField] private float invulnerabilityTime = 1f;

    private bool canTakeDamage = true;
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
    
    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnDamageTaken;
    public event Action OnDeath;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0 || damage <= 0 || isInvulnerable || !canTakeDamage)
            return;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(damage);
        if (currentHealth <= 0f)
        {
            Die();
            return;
        }
        StartCoroutine(InvulnerabilityCoroutine());
    }
    public void Heal(float healAmount)
    {
        if(healAmount <= 0 || currentHealth <= 0)
            return;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    [ContextMenu("Trigger Death")]
    public void TriggerDeath()
    {
        TakeDamage(maxHealth);
    }
    private void Die()
    {
        OnDeath?.Invoke();
    }
    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }
    public void SetCanTakeDamage(bool value)
    {
        canTakeDamage = value;
    }
}