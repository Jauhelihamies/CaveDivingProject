using UnityEngine;
using UnityEngine.InputSystem;

public class AutoReturnRotator : MonoBehaviour
{
    private enum RotationState { Idle, RotatingToTarget, Returning }
    private RotationState currentState = RotationState.Idle;

    [Header("Ohjausasetukset")]
    [Tooltip("Näppäin, jolla pyöriminen käynnistetään.")]
    public Key activationKey = Key.K; // Voit vaihtaa tämän Inspectorista!

    [Header("Pyörimisasetukset")]
    [Tooltip("Kulma, johon objekti pyörii (asteina).")]
    public float targetAngle = 45f;

    [Tooltip("Kuinka nopeasti objekti pyörii kohti kohdetta.")]
    public float speedToTarget = 150f;

    [Tooltip("Kuinka nopeasti objekti palaa takaisin alkuasentoon.")]
    public float returnSpeed = 100f;

    private Quaternion startingRotation;
    private Quaternion activeTargetRotation;


    void Start()
    {
        startingRotation = transform.rotation;
    }

    void Update()
    {
        // Varmistetaan, että näppäimistö on kytkettynä
        if (Keyboard.current == null) return;

        // 2. Unity 6 Input Systemin mukainen näppäinten luku direct-muodossa
        if (currentState == RotationState.Idle)
        {
            // Käytetään Inspectorista valittua näppäintä kiinteän Key.K sijaan
            if (Keyboard.current[activationKey].wasPressedThisFrame)
            {
                activeTargetRotation = startingRotation * Quaternion.Euler(0, 0, targetAngle);
                currentState = RotationState.RotatingToTarget;
            }
        }

        // 3. Pyörimislogiikka pysyy samana (toimii täydellisesti Unity 6:ssa)
        if (currentState == RotationState.RotatingToTarget)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                activeTargetRotation,
                speedToTarget * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, activeTargetRotation) < 0.1f)
            {
                currentState = RotationState.Returning;
            }
        }
        else if (currentState == RotationState.Returning)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                startingRotation,
                returnSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, startingRotation) < 0.1f)
            {
                transform.rotation = startingRotation;
                currentState = RotationState.Idle;
            }
        }
    }
}