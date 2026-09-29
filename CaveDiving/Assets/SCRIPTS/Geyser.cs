using UnityEngine;

public class Geyser2D : MonoBehaviour
{
    public Vector2 direction = Vector2.up;
    public float force = 20f;

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        rb.AddForce(direction.normalized * force * rb.mass, ForceMode2D.Force);
    }

    public void SetDirection(Vector2 newDir)
    {
        direction = newDir;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector3 start = transform.position;
        Vector3 dir = direction.normalized;
        float length = force * 0.1f;

        Vector3 end = start + dir * length;
        Gizmos.DrawLine(start, end);

        // Arrowhead
        Vector3 right = Quaternion.Euler(0, 0, 150) * dir;
        Vector3 left = Quaternion.Euler(0, 0, -150) * dir;
        float headSize = length * 0.25f;
        Gizmos.DrawLine(end, end + right * headSize);
        Gizmos.DrawLine(end, end + left * headSize);
    }
}