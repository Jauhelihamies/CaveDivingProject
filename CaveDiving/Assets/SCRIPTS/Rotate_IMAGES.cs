using UnityEngine;
using UnityEngine.UI; // Required for accessing the Image component

public class RotateAndFadeUI : MonoBehaviour
{
    public float rotationSpeed = 100f; 
    public float fadeDuration = 2.0f;  

    private Image uiImage;
    private float currentAlpha = 0f;

    void Start()
    {
        uiImage = GetComponent<Image>();
        if (uiImage != null)
        {
            Color c = uiImage.color;
            c.a = 0f;
            uiImage.color = c;
        }
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        if (uiImage != null && currentAlpha < 1f)
        {
            currentAlpha += Time.deltaTime / fadeDuration;
            currentAlpha = Mathf.Clamp01(currentAlpha); // Keeps value between 0 and 1

            Color c = uiImage.color;
            c.a = currentAlpha;
            uiImage.color = c;
        }
    }
}