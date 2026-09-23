using UnityEngine;
using UnityEngine.InputSystem;

public class LegMovement : MonoBehaviour
{
    public enum RotationState { Idle, RotatingToTarget, Returning }

    [System.Serializable]
    public class RotationObject
    {
        [Header("Objekti")]
        [Tooltip("Pyöritettävä kohde (Transform).")]
        public Transform targetTransform;

        [Header("Pyörimisasetukset")]
        [Tooltip("Kulma, johon objekti pyörii (asteina).")]
        public float targetAngle = 45f;

        [Tooltip("Kuinka nopeasti objekti pyörii kohti kohdetta.")]
        public float speedToTarget = 150f;

        [Tooltip("Kuinka nopeasti objekti palaa takaisin alkuasentoon.")]
        public float returnSpeed = 100f;

        [Tooltip("Viive sekunteina ennen kuin pyöriminen alkaa näppäimen painalluksesta.")]
        public float startDelay = 0f;

        // Sisäiset muuttujat objektin tilan seurantaan
        [HideInInspector] public RotationState currentState = RotationState.Idle;
        [HideInInspector] public Quaternion startingRotation;
        [HideInInspector] public Quaternion activeTargetRotation;
        [HideInInspector] public float delayTimer = 0f;
    }

    [Header("Ohjausasetukset")]
    [Tooltip("Näppäin, jolla pyöriminen käynnistetään kummallekin objektille.")]
    public Key activationKey = Key.K;

    [Header("Objektien hallinta")]
    public RotationObject objectOne;
    public RotationObject objectTwo;

    void Start()
    {
        // Alustetaan molempien objektien alkurotaatiot
        InitializeObject(objectOne);
        InitializeObject(objectTwo);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Kuunnellaan näppäinpainallusta
        if (Keyboard.current[activationKey].wasPressedThisFrame)
        {
            TriggerRotation(objectOne);
            TriggerRotation(objectTwo);
        }

        // Päivitetään molempien objektien logiikkaa itsenäisesti joka kehys
        UpdateObjectRotation(objectOne);
        UpdateObjectRotation(objectTwo);
    }

    private void InitializeObject(RotationObject obj)
    {
        if (obj.targetTransform != null)
        {
            obj.startingRotation = obj.targetTransform.rotation;
        }
    }

    private void TriggerRotation(RotationObject obj)
    {
        // Käynnistetään pyörimisprosessi vain, jos objekti on vapaana (Idle)
        if (obj.targetTransform != null && obj.currentState == RotationState.Idle)
        {
            obj.activeTargetRotation = obj.startingRotation * Quaternion.Euler(0, 0, obj.targetAngle);
            obj.delayTimer = obj.startDelay;

            // Siirrytään suoraan pyörimiseen jos viivettä ei ole, muuten odotetaan
            obj.currentState = RotationState.RotatingToTarget;
        }
    }

    private void UpdateObjectRotation(RotationObject obj)
    {
        if (obj.targetTransform == null || obj.currentState == RotationState.Idle) return;

        // Pyöriminen kohti kohdekulmaa
        if (obj.currentState == RotationState.RotatingToTarget)
        {
            // Jos viivettä on vielä jäljellä, vähennetään aikaa eikä pyöritetä vielä
            if (obj.delayTimer > 0f)
            {
                obj.delayTimer -= Time.deltaTime;
                return;
            }

            obj.targetTransform.rotation = Quaternion.RotateTowards(
                obj.targetTransform.rotation,
                obj.activeTargetRotation,
                obj.speedToTarget * Time.deltaTime
            );

            if (Quaternion.Angle(obj.targetTransform.rotation, obj.activeTargetRotation) < 0.1f)
            {
                obj.currentState = RotationState.Returning;
            }
        }
        // Paluu alkuasentoon
        else if (obj.currentState == RotationState.Returning)
        {
            obj.targetTransform.rotation = Quaternion.RotateTowards(
                obj.targetTransform.rotation,
                obj.startingRotation,
                obj.returnSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(obj.targetTransform.rotation, obj.startingRotation) < 0.1f)
            {
                obj.targetTransform.rotation = obj.startingRotation;
                obj.currentState = RotationState.Idle;
            }
        }
    }
}