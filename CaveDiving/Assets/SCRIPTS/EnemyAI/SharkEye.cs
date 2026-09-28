using System.Collections;
using UnityEngine;

public class SharkEyeMovement : MonoBehaviour
{
    public float wanderRadius = 0.05f;
    public float wanderSpeed = 1.5f;
    public float twitchInterval = 3f;
    [Range(0f, 1f)] public float twitchChance = 0.4f;
    public float twitchRadius = 0.15f;
    public float twitchHoldDurationMin = 0.4f;
    public float twitchHoldDurationMax = 0.9f;

    private Vector3 startPosition;
    private Vector3 currentModulationOffset;
    private float seedX;
    private float seedY;
    private bool isTwitching = false;

    void Start()
    {
        startPosition = transform.localPosition;
        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(0f, 100f);
        InvokeRepeating(nameof(TryTriggerTwitch), twitchInterval, twitchInterval);
    }

    void Update()
    {
        if (!isTwitching)
        {
            // Advance Perlin noise time
            float timeX = Time.time * wanderSpeed + seedX;
            float timeY = Time.time * wanderSpeed + seedY;

            // Generate smooth normal idle movement
            float noiseX = (Mathf.PerlinNoise(timeX, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, timeY) - 0.5f) * 2f;

            Vector3 normalOffset = new Vector3(noiseX, noiseY, 0) * wanderRadius;
            currentModulationOffset = Vector3.Lerp(currentModulationOffset, normalOffset, Time.deltaTime * 5f);
        }
        transform.localPosition = startPosition + currentModulationOffset;
    }

    void TryTriggerTwitch()
    {
        if (isTwitching) return;
        if (Random.value < twitchChance)
        {
            StartCoroutine(TwitchRoutine());
        }
    }

    private IEnumerator TwitchRoutine()
    {
        isTwitching = true;
        Vector3 twitchDirection = (Vector3)Random.insideUnitCircle.normalized * twitchRadius;
        currentModulationOffset = twitchDirection;
        float holdTime = Random.Range(twitchHoldDurationMin, twitchHoldDurationMax);
        yield return new WaitForSeconds(holdTime);
        float easeBackTimer = 0f;
        Vector3 snapBackStart = currentModulationOffset;

        while (easeBackTimer < 1f)
        {
            easeBackTimer += Time.deltaTime * 4f; 
            currentModulationOffset = Vector3.Lerp(snapBackStart, Vector3.zero, easeBackTimer);
            yield return null;
        }

        isTwitching = false;
    }
}