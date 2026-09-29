using System.Collections;
using UnityEngine;

public class SharkAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource musicSource; // Raahaa t‰h‰n AudioSource-komponentti

    [Header("Music Tracks")]
    public AudioClip chaseMusic;    // Raahaa t‰h‰n takaa-ajomusiikki (.mp3 / .wav)

    [Header("Fade Settings")]
    public float fadeOutDuration = 2.0f; // Kuinka monta sekuntia musiikin h‰ivytys kest‰‰
    public float postChaseHoldTime = 1.5f; // Kuinka kauan musiikki soi t‰ysill‰ jahdan p‰‰tytty‰ ennen kuin fade alkaa

    private float maxVolume;
    private Coroutine fadeCoroutine;
    private bool isChasing = false;

    void Start()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource != null)
        {
            maxVolume = musicSource.volume;
            musicSource.loop = true; // Varmistetaan ett‰ musiikki looppaa jahdan aikana
            musicSource.clip = chaseMusic;
        }
    }

    public void StartChaseMusic()
    {
        isChasing = true;

        // Jos h‰ivytys oli k‰ynniss‰, pys‰ytet‰‰n se
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.volume = maxVolume;
            musicSource.Play();
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

        musicSource.Stop();
        musicSource.volume = maxVolume; 
    }
}