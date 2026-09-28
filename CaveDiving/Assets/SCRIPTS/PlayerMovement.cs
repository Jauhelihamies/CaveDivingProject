using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveForce = 8f;
    [SerializeField] private float rotationTorque = 18f;
    [SerializeField] private float inputDelay = 0.3f;

    private Rigidbody2D rb;
    private bool isActing = false;
    public float Adrealine_Speed = 8f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb.linearDamping == 0) rb.linearDamping = 1.5f;
        if (rb.angularDamping == 0.05f) rb.angularDamping = 3f;
    }
    public void PlayerIsChased()
    {
        moveForce = Adrealine_Speed;
    }
    public void ChaseIsOver()
    {
        moveForce = 2f;
    }
    void Update()
    {
        if (Keyboard.current == null) return;
        if (isActing) return;
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            StartCoroutine(DelayedRotateRoutine(-rotationTorque));
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            StartCoroutine(DelayedRotateRoutine(rotationTorque));
        }
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            StartCoroutine(DelayedMoveRoutine(Vector2.down));
        }
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            StartCoroutine(DelayedMoveRoutine(Vector2.up));
        }
    }

    private IEnumerator DelayedMoveRoutine(Vector2 direction)
    {
        isActing = true;
        yield return new WaitForSeconds(inputDelay);


        Vector2 rotatedDirection = transform.TransformDirection(direction);

        rb.AddForce(rotatedDirection * moveForce, ForceMode2D.Impulse);

        isActing = false;
    }

    private IEnumerator DelayedRotateRoutine(float torqueValue)
    {
        isActing = true;
        yield return new WaitForSeconds(inputDelay);

        if (rb.IsSleeping()) rb.WakeUp();

        rb.AddTorque(torqueValue, ForceMode2D.Impulse);

        isActing = false;
    }
}