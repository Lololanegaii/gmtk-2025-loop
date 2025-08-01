using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource[] audioSources;
    public AudioClip pressPlayClick;
    public AudioClip playerDash;
    public AudioClip[] playerJump;
    public AudioClip portalEnter;
    public AudioClip portalExit;
    public AudioClip deathByPlatform;
    //
    public void Awake()
    {
        audioSources = GetComponents<AudioSource>();
    }
    //
    public void PlayAudio(AudioClip audio, float volume)
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                audioSources[i].pitch = Random.Range(0.8f, 1.2f);
                audioSources[i].PlayOneShot(audio, volume);
                break;
            }
        }
    }
    public void PlayAudioJump()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                audioSources[i].pitch = Random.Range(0.8f, 1.2f);
                audioSources[i].PlayOneShot(playerJump[Random.Range(0, playerJump.Length)], 0.64f);
                break;
            }
        }
    }
}
