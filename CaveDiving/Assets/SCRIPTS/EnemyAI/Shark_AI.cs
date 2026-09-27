using UnityEngine;

public class SharkAI : MonoBehaviour
{
    public enum SharkState { Patrolling, Chasing }

    [Header("State")]
    public SharkState currentState = SharkState.Patrolling;

    [Header("Movement & Patrol")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    [Header("Vision Settings")]
    public float visionRange = 8f;
    [Range(0, 180)] public float visionAngle = 60f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Rotation Settings")]
    public float rotationSpeed = 4f;
    [Tooltip("Kuinka monta astetta hai voi enimmill‰‰n kallistua yl‰- tai alaviistoon.")]
    public float maxClimbAngle = 45f;

    private Transform playerTransform;
    private bool facingRight = false; // Alkuper‰inen sprite katsoo vasemmalle
    private PlayerMovement cachedPlayerScript;
    [Header("Lost Delay Settings")]
    [Tooltip("Kuinka monta sekuntia hai jatkaa jahtaamista sen j‰lkeen, kun pelaaja katosi n‰kyvist‰.")]
    public float lostTargetDelay = 2.5f;
    private float lostTargetTimer = 0f;
    private bool canSeePlayerThisFrame = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            cachedPlayerScript = playerTransform.GetComponent<PlayerMovement>();
            if (cachedPlayerScript == null) cachedPlayerScript = playerTransform.GetComponentInChildren<PlayerMovement>();
            if (cachedPlayerScript == null) cachedPlayerScript = playerTransform.GetComponentInParent<PlayerMovement>();
        }
    }

    void Update()
    {
        CheckForPlayer();

        switch (currentState)
        {
            case SharkState.Patrolling:
                PatrolBehavior();
                break;
            case SharkState.Chasing:
                ChaseBehavior();
                break;
        }
    }

    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, patrolSpeed * Time.deltaTime);

        Vector2 moveDirection = (targetPoint.position - transform.position).normalized;

        HandleFacingAndRotation(moveDirection);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    void ChaseBehavior()
    {
        if (playerTransform == null) return;

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);

        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

        HandleFacingAndRotation(directionToPlayer);
    }

    void HandleFacingAndRotation(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        if (direction.x > 0.1f && !facingRight)
        {
            facingRight = true;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (direction.x < -0.1f && facingRight)
        {
            facingRight = false;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x); 
            transform.localScale = scale;
        }

        transform.rotation = Quaternion.identity;
    }

    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        canSeePlayerThisFrame = false; // Nollataan joka framen alussa

        if (distanceToPlayer <= visionRange)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector2 forwardDirection = -transform.right;

            float angleToPlayer = Vector2.Angle(forwardDirection, directionToPlayer);

            // Tarkistetaan onko pelaaja n‰kˆkent‰n sis‰ll‰
            if (angleToPlayer < visionAngle / 2f)
            {
                // Tarkistetaan esteet (Linecast)
                RaycastHit2D hit = Physics2D.Linecast(transform.position, playerTransform.position, obstacleMask | playerMask);

                if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                {
                    canSeePlayerThisFrame = true;
                }
            }
        }
        if (canSeePlayerThisFrame)
        {

            lostTargetTimer = lostTargetDelay;

            if (currentState != SharkState.Chasing)
            {
                OnPlayerSpotted();
            }
        }
        else
        {
            if (currentState == SharkState.Chasing)
            {
                lostTargetTimer -= Time.deltaTime;
                if (lostTargetTimer <= 0f)
                {
                    OnPlayerLost();
                }
            }
        }
    }

    void OnPlayerSpotted()
    {
        currentState = SharkState.Chasing;
        cachedPlayerScript?.PlayerIsChased();
    }

    void OnPlayerLost()
    {
        currentState = SharkState.Patrolling;
        transform.rotation = Quaternion.identity;
        cachedPlayerScript?.ChaseIsOver();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentState == SharkState.Chasing ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 forward = -transform.right; // Korjattu osoittamaan hain kuonon suuntaan
        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2f) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2f) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionRange);
    }
}