using UnityEngine;
using Random = UnityEngine.Random;

public class AudioEmitter : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;

    private readonly float minTimeBetweenSounds = 0.1f;
    private float sfxLastPlayTime;
    
    public void PlayRandomSfx(AudioClip[] clips, float volume = 1)
    {
        if (Time.time - sfxLastPlayTime < minTimeBetweenSounds)
            return;
        sfxLastPlayTime = Time.time;
        
        if(clips.Length <= 0 || sfxAudioSource == null) return;
        
        sfxAudioSource.pitch = Random.Range(0.95f, 1.05f);
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        if(randomClip == null) return;
        sfxAudioSource.PlayOneShot(randomClip, volume);
    }
    public void PlaySfx(AudioClip clip, float volume = 1)
    {
        if(Time.time - sfxLastPlayTime < minTimeBetweenSounds)
            return; 
        sfxLastPlayTime = Time.time;
        
        if(clip  == null || sfxAudioSource == null) return;
        sfxAudioSource.pitch = Random.Range(0.95f, 1.05f);
        sfxAudioSource.PlayOneShot(clip, volume);
    }
    
}