using System.Collections;
using UnityEngine;

public class ImageFader : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetSprite;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float visibleDuration = 1.0f;
    [SerializeField] private float hiddenDuration = 0.5f;
    [SerializeField] private int fadeCycles = 3;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip startSound;    

    private Coroutine fadeCoroutine;
    public FADE_IMAGE FI;
    public MainMenuEffects MainMenuEffectsX;

    private void Start()
    {
        if (targetSprite != null)
        {
            Color currentColor = targetSprite.color;
            currentColor.a = 0f;
            targetSprite.color = currentColor;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void StartGame()
    {

        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound);
        }

        MainMenuEffectsX.StartSmokes();

        FI.FADE_IMAGE_FUNCTION();
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        for (int i = 0; i < fadeCycles; i++)
        {
            // 1. Häivytys näkyviin (Fade In)
            yield return StartCoroutine(FadeSprite(0f, 1f));

            // 2. Pysyy näkyvissä
            yield return new WaitForSeconds(visibleDuration);

            // 3. Häivytys piiloon (Fade Out)
            yield return StartCoroutine(FadeSprite(1f, 0f));

            if (i < fadeCycles - 1)
            {
                yield return new WaitForSeconds(hiddenDuration);
            }
        }
    }

    private IEnumerator FadeSprite(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        Color currentColor = targetSprite.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            currentColor.a = newAlpha;
            targetSprite.color = currentColor;
            yield return null;
        }

        currentColor.a = targetAlpha;
        targetSprite.color = currentColor;
    }
}