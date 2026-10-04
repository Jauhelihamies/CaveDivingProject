using UnityEngine;
using UnityEngine.Audio;

public class EnemyVesselController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 150f;
    [SerializeField] private float minY = -180f;
    [SerializeField] private float maxY = 180f;

    [Header("Spawning")]
    [SerializeField] private GameObject standardTorpedoPrefab;
    [SerializeField] private GameObject fastTorpedoPrefab;
    [SerializeField] private float spawnRate = 2f;
    [Range(0f, 1f)][SerializeField] private float fastTorpedoChance = 0.15f;
    [SerializeField] private float spawnOffsetX = -60f; 

    [Header("Health & Boss Settings")]
    [SerializeField] private int maxHealth = 5; 

    private int currentHealth;
    private float spawnTimer = 0f;
    private int moveDirection = 1; 
    private RectTransform rectTransform;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        HandleMovement();
        HandleSpawning();
    }

    void HandleMovement()
    {
        Vector3 pos = rectTransform.anchoredPosition;
        pos.y += moveDirection * moveSpeed * Time.deltaTime;

        // Ping-pong movement between boundaries
        if (pos.y >= maxY)
        {
            pos.y = maxY;
            moveDirection = -1;
        }
        else if (pos.y <= minY)
        {
            pos.y = minY;
            moveDirection = 1;
        }

        rectTransform.anchoredPosition = pos;
    }

    void HandleSpawning()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate)
        {
            SpawnTorpedo();
            spawnTimer = 0f;
        }
    }

    void SpawnTorpedo()
    {
        GameObject prefabToSpawn = standardTorpedoPrefab;
        if (Random.value < fastTorpedoChance && fastTorpedoPrefab != null)
        {
            prefabToSpawn = fastTorpedoPrefab;
        }

        GameObject newTorpedo = Instantiate(prefabToSpawn, transform.parent);
        RectTransform torpedoRect = newTorpedo.GetComponent<RectTransform>();


        Vector2 spawnPos = rectTransform.anchoredPosition;
        spawnPos.x += spawnOffsetX;
        torpedoRect.anchoredPosition = spawnPos;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (currentHealth <= 0)
        {
            SubManager.Instance.TriggerWin();

            Destroy(gameObject);
        }
    }
}