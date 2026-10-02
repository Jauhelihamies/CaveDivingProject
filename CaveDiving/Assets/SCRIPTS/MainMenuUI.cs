using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Tarvitaan Coroutinea varten

public class MainMenuUI : MonoBehaviour
{
    public ImageFader StartAlarmSystem;
    public MainMenu GetFleeCode;

    [Header("Audio Settings")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public float fadeDuration = 1.0f; 

    public void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return; 

        Vector2 mousepos = Mouse.current.position.ReadValue();
        Ray mouseRay = cam.ScreenPointToRay(mousepos);
        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo))
        {
            if (hitInfo.collider.gameObject.CompareTag("Start"))
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    StartAlarmSystem.StartGame();
                    // K‰ynnistet‰‰n ‰‰nen h‰ivytys
                    StartCoroutine(FadeOutAudio());
                }
            }
            if (hitInfo.collider.gameObject.CompareTag("Quit"))
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    GetFleeCode.Flee();
                    // K‰ynnistet‰‰n ‰‰nen h‰ivytys
                    StartCoroutine(FadeOutAudio());
                }
            }
        }
    }

    private IEnumerator FadeOutAudio()
    {
        // Otetaan talteen alkutilanteen ‰‰nenvoimakkuudet
        float startVolume1 = audioSource1 != null ? audioSource1.volume : 0;
        float startVolume2 = audioSource2 != null ? audioSource2.volume : 0;

        float currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;


            if (audioSource1 != null)
            {
                audioSource1.volume = Mathf.Lerp(startVolume1, 0, currentTime / fadeDuration);
            }
            if (audioSource2 != null)
            {
                audioSource2.volume = Mathf.Lerp(startVolume2, 0, currentTime / fadeDuration);
            }

            yield return null; // Odotetaan seuraavaa framea
        }

        // Varmistetaan, ett‰ ‰‰net ovat t‰ysin nollassa ja pys‰ytet‰‰n ne
        if (audioSource1 != null)
        {
            audioSource1.volume = 0;
            audioSource1.Stop();
        }
        if (audioSource2 != null)
        {
            audioSource2.volume = 0;
            audioSource2.Stop();
        }
    }
}