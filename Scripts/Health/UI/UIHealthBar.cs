using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image ghostHealthBar;
    [SerializeField] private Image healthBarBackground;
    private Health health;
    [SerializeField] bool showHealthBar = true;
    [SerializeField] private CanvasGroup damageOverlayCanvasGroup;
    
    private Coroutine damageOverlayCoroutine;
    private Coroutine ghostBarCoroutine;
    private float baseDamageOverlayAlpha;
    private float lastHealthPercent;
    
    //ghost bar animation
    private float ghostBarBlendDuration = 0.5f;
    private float ghostBarDelay = 0.5f;
    
    //Damage overlay animation
    private float durationIn = 0.1f;
    private float durationOut = 0.4f;

    private void Awake()
    {
        if (!showHealthBar)
        {
            healthBar.color = Color.clear;
            ghostHealthBar.color = Color.clear;
            healthBarBackground.color = Color.clear;
        }
    }
    private void OnEnable()
    {
        if (health)
        {
            health.OnHealthChanged -= OnHealthChanged; //prevents double subscription
            health.OnDamageTaken -= ShowDamageFeedback;
            health.OnHealthChanged += OnHealthChanged;
            health.OnDamageTaken += ShowDamageFeedback;
        }
    }
    private void OnDisable()
    {
        if(!health) 
            return;
        health.OnHealthChanged -= OnHealthChanged;
        health.OnDamageTaken -= ShowDamageFeedback;
    }
    public void Initialize(Health health)
    {
        this.health = health;
        
        health.OnHealthChanged -= OnHealthChanged; //prevent double subscription (uneeded, initialize is only called on awake)
        health.OnDamageTaken -= ShowDamageFeedback;
        health.OnHealthChanged += OnHealthChanged;
        health.OnDamageTaken += ShowDamageFeedback;
        
        //set initial state, removed update health bar call
        healthBar.fillAmount = health.HealthPercent;
        ghostHealthBar.fillAmount = health.HealthPercent;
        lastHealthPercent = health.HealthPercent;
    }
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthBar();
    }
    private void UpdateHealthBar()
    {
        healthBar.fillAmount = health.HealthPercent;
        if(health.HealthPercent < lastHealthPercent)
            TriggerGhostBarAnim();
        else
            ghostHealthBar.fillAmount = health.HealthPercent;
            
        lastHealthPercent = health.HealthPercent;
    }
    private void ShowDamageFeedback(float damage)
    {
        if(damageOverlayCoroutine != null)
            StopCoroutine(damageOverlayCoroutine);
        damageOverlayCoroutine = StartCoroutine(DamageAlphaBlend());
    }
    
    private void TriggerGhostBarAnim()
    {
        if(ghostBarCoroutine != null)
            StopCoroutine(ghostBarCoroutine);
        ghostBarCoroutine = StartCoroutine(GhostBarAnim());
    }
    private System.Collections.IEnumerator DamageAlphaBlend()
    {
        float t = 0;
        float initialAlpha = damageOverlayCanvasGroup.alpha;
        float maxAlpha = Mathf.Lerp(1f, 0.1f, health.HealthPercent);

        while (t < durationIn)
        {
            t += Time.deltaTime;
            damageOverlayCanvasGroup.alpha = Mathf.Lerp(initialAlpha, maxAlpha, t / durationIn);
            yield return null;
        }
        t = 0;
        while (t < durationOut)
        {
            t += Time.deltaTime;
            //return to base alpha, in case the coroutine is started when alpha was in the middle of blending
            damageOverlayCanvasGroup.alpha = Mathf.Lerp(maxAlpha, baseDamageOverlayAlpha, t / durationOut);
            yield return null;
        }
        damageOverlayCanvasGroup.alpha = baseDamageOverlayAlpha;
    }
    private System.Collections.IEnumerator GhostBarAnim()
    {
        yield return new WaitForSeconds(ghostBarDelay);
        
        float target = health.HealthPercent;
        
        float t = 0f;
        float initialValue = ghostHealthBar.fillAmount;

        while (t < ghostBarBlendDuration)
        {
            t += Time.deltaTime;
            float ease =  t / ghostBarBlendDuration;
            ease *= ease;
            ghostHealthBar.fillAmount = Mathf.Lerp(initialValue, target, ease);
            yield return null;
        }
        ghostHealthBar.fillAmount = target;
    }
}
