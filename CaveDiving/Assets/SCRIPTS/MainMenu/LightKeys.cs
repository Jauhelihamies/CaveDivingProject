using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Required for the New Input System

public class KeyGlow : MonoBehaviour
{
    [Header("Key Configuration")]
    [Tooltip("The specific key using the New Input System enum.")]
    public Key targetKey = Key.Q;

    [Header("Glow Appearance")]
    public Color normalColor = Color.white;
    public Color glowColor = Color.yellow;

    [Range(1f, 10f)]
    [Tooltip("Multiplies the glow color brightness. Values > 1 create an HDR glow for Bloom effects.")]
    public float intensity = 2.0f;

    private SpriteRenderer spriteRenderer;
    private Image uiImage;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();
    }

    void Update()
    {
        // Ensure a keyboard is connected to prevent null reference errors
        if (Keyboard.current == null) return;

        // Get the specific key control dynamically from the keyboard
        var keyControl = Keyboard.current[targetKey];

        // Use .isPressed for "while holding down". 
        // Use keyControl.wasPressedThisFrame if you only want a quick flash on tap.
        if (keyControl != null && keyControl.isPressed)
        {
            // Multiply RGB channels by intensity to create an HDR glow value
            Color hdrGlow = new Color(
                glowColor.r * intensity,
                glowColor.g * intensity,
                glowColor.b * intensity,
                glowColor.a
            );

            ApplyColor(hdrGlow);
        }
        else
        {
            ApplyColor(normalColor);
        }
    }

    private void ApplyColor(Color color)
    {
        if (spriteRenderer != null) spriteRenderer.color = color;
        if (uiImage != null) uiImage.color = color;
    }
}