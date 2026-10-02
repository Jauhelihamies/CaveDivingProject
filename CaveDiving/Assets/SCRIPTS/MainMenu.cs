using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour

{
    [Header("Scene")]
    [SerializeField] private string caveSceneName = "GameScene";

    [Header("Audio Components")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip riserClip;
    [SerializeField] private AudioClip screamClip;

    [Header("Jumpscare Timing")]
    [Tooltip("Piinaavan hiljaisuuden kesto sekunteina riser-äänen jälkeen ennen säikäytystä.")]
    [SerializeField] private float delayBeforeScream = 1.0f; // UUSI: Hiljaisuuden säätö

    [Header("Jumpscare Visuals")]
    [SerializeField] private RectTransform sharkImage;
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float endScale = 3f;
    [SerializeField] private float growTime = 0.35f;
    [SerializeField] private float holdTime = 1.0f;

    private bool fleeing;

    public void Descend()
    {
        if (fleeing) return;
        SceneManager.LoadScene(caveSceneName);
    }

    public void Flee()
    {
        if (fleeing) return;
        fleeing = true;
        StartCoroutine(JumpscareThenQuit());
    }

    private IEnumerator JumpscareThenQuit()
    {
        if (audioSource != null && riserClip != null)
        {
            audioSource.PlayOneShot(riserClip);
            yield return null;
        }

        // 2. UUSI: Piinaava hiljaisuus jännitysäänen jälkeen
        if (delayBeforeScream > 0f)
        {
            yield return new WaitForSecondsRealtime(delayBeforeScream);
        }

        // 3. Aktivoidaan säikäytyskuva ja huuto
        sharkImage.gameObject.SetActive(true);
        sharkImage.localScale = Vector3.one * startScale;

        if (audioSource != null && screamClip != null)
        {
            audioSource.PlayOneShot(screamClip);
        }

        // 4. Kuvan kasvatus-animaatio
        float t = 0f;
        while (t < growTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / growTime);
            float eased = p * p;
            sharkImage.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);
            yield return null;
        }

        sharkImage.localScale = Vector3.one * endScale;
        yield return new WaitForSecondsRealtime(holdTime);

        Quit();
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}