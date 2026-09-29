using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class FADE_IMAGE : MonoBehaviour
{

    public SpriteRenderer fadeSprite;

    [Header("Viiveet ja kestot")]
    public float startDelay = 1.0f;
    public float fadeDuration = 1.0f;
    public float StartCount = 3.0f;

    [Header("Asetukset")]
    public string Level_Name;

    private bool isFading = false;

    public void Start()
    {
        if (fadeSprite != null)
        {
            fadeSprite.gameObject.SetActive(true);
            Color color = fadeSprite.color;
            color.a = 0;
            fadeSprite.color = color;
        }
    }

    public void FADE_IMAGE_FUNCTION()
    {
        if (fadeSprite != null && !isFading)
        {
            StartCoroutine(FadeInImage());
        }
    }

    IEnumerator FadeInImage()
    {
        isFading = true;

        yield return new WaitForSeconds(startDelay);

        float elapsedTime = 0;
        Color color = fadeSprite.color;

        color.a = 0;
        fadeSprite.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            fadeSprite.color = color;
            yield return null;
        }

        color.a = 1;
        fadeSprite.color = color;

        yield return new WaitForSeconds(StartCount);

        isFading = false;

        SceneManager.LoadScene(Level_Name);
    }
}