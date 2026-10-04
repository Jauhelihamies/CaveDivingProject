using UnityEngine;

public class PlayerTorpedo : MonoBehaviour
{
    [SerializeField] private float speed = 500f;
    [SerializeField] private float screenRightBound = 500f; 

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Move Right constantly
        rectTransform.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        if (rectTransform.anchoredPosition.x > screenRightBound)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
   
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }

        else if (collision.CompareTag("Boss")) 
        {
            EnemyVesselController boss = collision.GetComponent<EnemyVesselController>();
            if (boss != null)
            {
                boss.TakeDamage(1);
            }
            Destroy(gameObject);
        }
    }
}