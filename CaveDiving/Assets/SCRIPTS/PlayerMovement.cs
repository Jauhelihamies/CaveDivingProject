using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 8f;
    [SerializeField] private float rotationTorque = 18f;
    [SerializeField] private float inputDelay = 0.3f;

    private Rigidbody2D rb;
    private bool isActing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // FIX: Using the updated, modern Unity physics naming conventions
        if (rb.linearDamping == 0) rb.linearDamping = 1.5f;
        if (rb.angularDamping == 0.05f) rb.angularDamping = 3f;
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (isActing) return;

        // A rotates clockwise
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            StartCoroutine(DelayedRotateRoutine(-rotationTorque));
        }
        // D rotates counter-clockwise
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            StartCoroutine(DelayedRotateRoutine(rotationTorque));
        }
        // UpArrow moves down (relative to player orientation)
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            StartCoroutine(DelayedMoveRoutine(Vector2.down));
        }
        // DownArrow moves up (relative to player orientation)
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
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