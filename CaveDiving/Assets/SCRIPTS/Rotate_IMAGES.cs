using UnityEngine;
using UnityEngine.UI; // Required for accessing the Image component

public class RotateAndFadeUI : MonoBehaviour
{
    public float rotationSpeed = 100f; // Degrees per second
    public float fadeDuration = 2.0f;  // Time in seconds to fully fade in

    private Image uiImage;
    private float currentAlpha = 0f;

    void Start()
    {
        // Get the Image component and set its initial color to completely transparent
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
        // 1. Rotate around the Z-axis
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 2. Smoothly increase Alpha until it reaches 1 (fully visible)
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