using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class FadeInWithDelay : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    public float startDelay = 1.5f;

    void Start()
    {
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(startDelay);

        float elapsedTime = 0;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 0;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }
}