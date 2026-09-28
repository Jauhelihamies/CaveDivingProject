using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string caveSceneName = "GameScene";

    [Header("Jumpscare")]
    [SerializeField] private RectTransform sharkImage;
    [SerializeField] private AudioSource screamSource;
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
        sharkImage.gameObject.SetActive(true);
        sharkImage.localScale = Vector3.one * startScale;

        if (screamSource != null) screamSource.Play();

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