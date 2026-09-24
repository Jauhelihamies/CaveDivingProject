using UnityEngine;

public class Explosion : MonoBehaviour
{

    public float explosionRadius = 5f;       
    public float explosionForce = 500f;      
    public GameObject explosionEffectPrefab;
    public GameObject SmokePrefab;
    public GameObject Bubbles;
    public void ExplosionEffect()
    {

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Instantiate(SmokePrefab, transform.position, Quaternion.identity);
            Instantiate(Bubbles, transform.position, Quaternion.identity);
        }
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in colliders)
        {

            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();

            if (rb != null)
            {

                Vector2 direction = rb.transform.position - transform.position;

                // Lasketaan etäisyys (mitä lähempänä, sitä kovempi isku)
                float distance = direction.magnitude;

                if (distance > 0)
                {
                    direction.Normalize();
                    float forceMultiplier = (explosionRadius - distance) / explosionRadius;
                    rb.AddForce(direction * explosionForce * forceMultiplier);
                }
            }
        }

    }
}
