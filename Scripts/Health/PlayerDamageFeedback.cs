using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerDamageFeedback : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private bool cameraShake = true;
    [SerializeField] private bool playDamageSound = true;
    [SerializeField] private bool dissolveEffect = true;
    [SerializeField] private bool playDeathAnimation = true;
    
    [SerializeField] private DissolveEffectHandler dissolveEffectHandler;
    
    [SerializeField] private AudioEmitter playerAudio;
    [SerializeField] private AudioClip[] damageClips;

    private void OnEnable()
    {
        if (!health)
            return;
        health.OnDamageTaken += ShowDamageFeedback;
        health.OnDeath += OnDeath;
    }
    private void OnDisable()
    {
        if (!health)
            return;
        health.OnDamageTaken -= ShowDamageFeedback;
        health.OnDeath -= OnDeath;
    }
    private void ShowDamageFeedback(float damage)
    {
        if(cameraShake) impulseSource?.GenerateImpulse();
        if(playDamageSound) playerAudio?.PlayRandomSfx(damageClips);
    }
    private void OnDeath()
    {
        if(playDeathAnimation) playerAnimation?.PlayDeath();
        if(dissolveEffect) dissolveEffectHandler?.DissolveShader();
        if (TryGetComponent(out CharacterController c)) c.enabled = false;
    }
}
