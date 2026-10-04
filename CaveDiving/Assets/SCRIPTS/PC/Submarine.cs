using UnityEngine;
using UnityEngine.InputSystem;

public class SubmarineController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 300f;
    [SerializeField] private float minY = -180f;
    [SerializeField] private float maxY = 180f;

    [Header("Weapons")]
    [SerializeField] private GameObject playerTorpedoPrefab;
    [SerializeField] private float fireCooldown = 0.75f;
    [SerializeField] private float spawnOffsetX = 60f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private RectTransform rectTransform;
    private float nextFireTime = 0f;

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
        HandleShooting();
    }

    void HandleMovement()
    {
        float direction = 0f;
        if (Keyboard.current.qKey.isPressed) direction = 1f;
        else if (Keyboard.current.wKey.isPressed) direction = -1f;

        if (direction != 0f)
        {
            Vector3 newPos = rectTransform.anchoredPosition;
            newPos.y += direction * moveSpeed * Time.deltaTime;
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            rectTransform.anchoredPosition = newPos;
        }
    }

    void HandleShooting()
    {
        if ((Keyboard.current.spaceKey.isPressed) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        if (playerTorpedoPrefab == null) return;
        GameObject ammo = Instantiate(playerTorpedoPrefab, transform.parent);
        RectTransform ammoRect = ammo.GetComponent<RectTransform>();
        Vector2 spawnPosition = rectTransform.anchoredPosition;
        spawnPosition.x += spawnOffsetX;
        ammoRect.anchoredPosition = spawnPosition;
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
            SubManager.Instance.TriggerLoss();

            gameObject.SetActive(false);
        }
    }
}