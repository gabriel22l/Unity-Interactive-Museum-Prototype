using UnityEngine;

public class FootstepAudioEvents : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] terrainFootstepClips;
    [SerializeField] private AudioClip[] groundFootstepClips;
    
    [SerializeField] private LayerMask groundLayerMask;

    public void OnFootstep(AnimationEvent e)
    {
        if (e.animatorClipInfo.weight < 0.5)
            return;
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 2f, groundLayerMask))
            PlayRandomClip(groundFootstepClips);
        else
            PlayRandomClip(terrainFootstepClips, 0.5f); //fallback to terrain/forest sounds
    }
    private void PlayRandomClip(AudioClip[] clips, float volume = 1)
    {
        if(clips.Length <= 0)
            return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
        {
            Debug.LogError("null clip");
            return;
        }
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, volume);
    }
}
