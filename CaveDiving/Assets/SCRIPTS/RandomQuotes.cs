using System.Collections;
using TMPro;
using UnityEngine;

public class RandomQuotes : MonoBehaviour
{
    [SerializeField] private string[] Quotes;
    [SerializeField] private TextMeshProUGUI TEXT;
    [SerializeField] private float fadeDuration = 1.0f; // Häivytyksen kesto sekunteina
    [SerializeField] private float startDelay = 2.0f;    // Viive pelin alussa sekunteina

    private Coroutine fadeCoroutine;

    void Start()
    {

        if (TEXT != null)
        {
            TEXT.text = "";
        }
        StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        RandomQuoteGeneration();
    }

    public void RandomQuoteGeneration()
    {
        if (Quotes == null || Quotes.Length == 0) return;

        int RIn = Random.Range(0, Quotes.Length);
        string SelectedQ = Quotes[RIn];

        if (TEXT != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeInText(SelectedQ));
        }
    }

    private IEnumerator FadeInText(string newText)
    {
        TEXT.text = newText;

        Color textColor = TEXT.color;
        textColor.a = 0;
        TEXT.color = textColor;

        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(0f, 1f, currentTime / fadeDuration);
            TEXT.color = textColor;
            yield return null;
        }

        textColor.a = 1f;
        TEXT.color = textColor;
    }
}