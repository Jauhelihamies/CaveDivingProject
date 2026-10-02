using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class RetroLoadingBar : MonoBehaviour
{

    public RectTransform fillRectTransform;
    public TMP_Text percentageText;

    public float maxBarWidth = 400f;
    public float startDelay = 1.5f;
    public float GameLauchDelay = 1f;
    public string MM = "MainMenu";
    public float speedMultiplier = 1.0f;

    private float currentProgress = 0f;

    void Start()
    {
        if (fillRectTransform != null)
        {
            fillRectTransform.sizeDelta = new Vector2(0f, fillRectTransform.sizeDelta.y);

            if (percentageText != null)
                percentageText.text = "0%";

            StartCoroutine(StartRealisticLoading());
        }
    }

    IEnumerator StartRealisticLoading()
    {

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        while (currentProgress < 1.0f)
        {
            // Valitaan satunnainen pyrähdys
            float nextTarget = Mathf.Min(currentProgress + Random.Range(0.08f, 0.25f), 1.0f);

            // Perusnopeus kerrotaan Inspectorissa annetulla speedMultiplierilla
            float speed = Random.Range(0.6f, 2.0f) * speedMultiplier;

            // 2. Liikutetaan palkkia kohti välitavoitetta
            while (currentProgress < nextTarget)
            {
                currentProgress = Mathf.MoveTowards(currentProgress, nextTarget, Time.deltaTime * speed);

                float currentWidth = currentProgress * maxBarWidth;
                fillRectTransform.sizeDelta = new Vector2(currentWidth, fillRectTransform.sizeDelta.y);

                if (percentageText != null)
                {
                    int percentRounded = Mathf.RoundToInt(currentProgress * 100f);
                    percentageText.text = percentRounded + "%";
                }

                yield return null;
            }
            if (currentProgress < 1.0f)
            {
                float pauseDuration = Random.Range(0.1f, 0.5f) / speedMultiplier;
                yield return new WaitForSeconds(pauseDuration);
            }
        }

        if (percentageText != null)
            percentageText.text = "100%";
        StartCoroutine(GameLaunch());


    }
    IEnumerator GameLaunch()
    {
        yield return new WaitForSeconds(GameLauchDelay);
        SceneManager.LoadScene(MM);
    }
}