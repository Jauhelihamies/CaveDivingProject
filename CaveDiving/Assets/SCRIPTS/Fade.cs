using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
   
    public SpriteRenderer fadeSprite;
    public float fadeDuration = 1.0f;
    public float StartCount = 3.0f;
    public string Level_Name;
    
    public void Start()
    {
        if (fadeSprite != null)
        {
            fadeSprite.gameObject.SetActive(false);

        }
    }

    public void FADEout()
    {
        if (fadeSprite != null)
        {

            StartCoroutine(FadeInImage());
        }
    }

    IEnumerator FadeInImage()
    {
        fadeSprite.gameObject.SetActive(true);
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
        SceneManager.LoadScene(Level_Name);
        yield return null;
    }
}