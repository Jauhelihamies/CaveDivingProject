using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [Tooltip("Kuinka monta sekuntia riser-äänen alkamisesta odotetaan ennen taustan vaihtoa. Tämän jälkeen siirrytään heti hiljaisuuteen.")]
    [SerializeField] private float backgroundChangeDelay = 2.0f;
    [Tooltip("Piinaavan hiljaisuuden kesto sekunteina taustan vaihdon jälkeen ennen säikäytystä.")]
    [SerializeField] private float delayBeforeScream = 1.0f;

    [Header("Jumpscare Visuals")]
    [SerializeField] private RectTransform sharkImage;
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float endScale = 3f;
    [SerializeField] private float growTime = 0.35f;
    [SerializeField] private float holdTime = 1.0f;

    [Header("Backgrounds")]
    public GameObject TensionBackGround;

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


            yield return new WaitForSecondsRealtime(backgroundChangeDelay);


            if (TensionBackGround != null)
            {
                TensionBackGround.gameObject.SetActive(true);
            }

        }

        if (delayBeforeScream > 0f)
        {
            yield return new WaitForSecondsRealtime(delayBeforeScream);
        }


        if (sharkImage != null)
        {
            sharkImage.gameObject.SetActive(true);
            sharkImage.localScale = Vector3.one * startScale;
        }

        if (audioSource != null && screamClip != null)
        {
            audioSource.PlayOneShot(screamClip);
        }

        // Kuvan kasvatus-animaatio
        float t = 0f;
        while (t < growTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / growTime);
            float eased = p * p;
            if (sharkImage != null)
            {
                sharkImage.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);
            }
            yield return null;
        }

        if (sharkImage != null)
        {
            sharkImage.localScale = Vector3.one * endScale;
        }

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