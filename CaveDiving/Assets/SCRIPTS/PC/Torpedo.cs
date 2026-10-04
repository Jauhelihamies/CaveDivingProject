using UnityEngine;

public class TorpedoController : MonoBehaviour
{
    [SerializeField] private float speed = 400f;
    [SerializeField] private float screenLeftBound = -500f; // Where the torpedo gets destroyed

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {

        rectTransform.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        // Destroy the torpedo once it leaves the mini-game frame area
        if (rectTransform.anchoredPosition.x < screenLeftBound)
        {
            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SubmarineController player = collision.GetComponent<SubmarineController>();
            if (player != null)
            {
                player.TakeDamage(1); 
            }
            Destroy(gameObject); 
        }
    }
}