using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("SFX")]
    public AudioClip jumpSound;
    public AudioClip flipSound;
    public AudioClip landingSound;
    public AudioClip gameOverSound;

    [Header("Footsteps")]
    public AudioClip[] grassSteps;
    public AudioClip[] rockSteps;

    void Awake() { instance = this; }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayFootstep(string surface)
    {
        AudioClip clip = null;
        if (surface == "Grass" && grassSteps.Length > 0) clip = grassSteps[Random.Range(0, grassSteps.Length)];
        else if (surface == "Rock" && rockSteps.Length > 0) clip = rockSteps[Random.Range(0, rockSteps.Length)];

        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.85f, 1.15f);
            sfxSource.PlayOneShot(clip, 0.4f);
        }
    }

    public void TriggerGameOver()
    {
        if (musicSource != null) musicSource.Stop();
        if (gameOverSound != null) sfxSource.PlayOneShot(gameOverSound);
    }
}