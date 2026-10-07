using System.Collections;
using UnityEngine;

public class SharkAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource musicSource;

    [Header("Music Tracks")]
    public AudioClip chaseMusic;

    [Header("Fade Settings")]
    public float fadeOutDuration = 2.0f;
    public float postChaseHoldTime = 1.5f;

    private float maxVolume;
    private Coroutine fadeCoroutine;
    private bool isChasing = false;

    void Start()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource != null && chaseMusic != null)
        {
            maxVolume = musicSource.volume;
            musicSource.loop = true;
            musicSource.clip = chaseMusic;
            chaseMusic.LoadAudioData(); // Pakotetaan data RAM-muistiin
            StartCoroutine(WarmUpAudioBuffer());
        }
    }


    private IEnumerator WarmUpAudioBuffer()
    {
        musicSource.volume = 0f;
        musicSource.Play();
        yield return null; 
        musicSource.Pause(); 
        musicSource.volume = maxVolume;
    }

    public void StartChaseMusic()
    {
        isChasing = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (musicSource != null)
        {
            musicSource.volume = maxVolume;
            if (!musicSource.isPlaying)
            {
                musicSource.UnPause();
                if (!musicSource.isPlaying) musicSource.Play();
            }
        }
    }

    public void StopChaseMusic()
    {
        isChasing = false;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return new WaitForSeconds(postChaseHoldTime);
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            if (isChasing) yield break;

            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
            yield return null;
        }
        musicSource.Pause();
        musicSource.volume = maxVolume;
    }
}