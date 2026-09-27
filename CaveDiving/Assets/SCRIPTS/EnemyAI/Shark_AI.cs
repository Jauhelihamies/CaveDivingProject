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

    [Header("Obstacle Avoidance")]
    [Tooltip("Kuinka kauas eteenpäin hai katsoo seinien varalta.")]
    public float avoidanceDistance = 2f;
    [Tooltip("Kuinka voimakkaasti hai väistää seinää ylös/alas.")]
    public float avoidanceForce = 3f;

    private Transform playerTransform;
    private bool facingRight = false; // Alkuperäinen sprite katsoo vasemmalle
    private PlayerMovement cachedPlayerScript;
    private Collider2D sharkCollider;

    [Header("Lost Delay Settings")]
    [Tooltip("Kuinka monta sekuntia hai jatkaa jahtaamista sen jälkeen, kun pelaaja katosi näkyvistä.")]
    public float lostTargetDelay = 2.5f;
    private float lostTargetTimer = 0f;
    private bool canSeePlayerThisFrame = false;

    void Start()
    {
        sharkCollider = GetComponent<Collider2D>();

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

        // Lasketaan perussuunta kohteeseen
        Vector2 moveDirection = (targetPoint.position - transform.position).normalized;

        // --- ESTEIDEN VÄISTÖ (Avoidance) ---
        Vector2 forwardVector = facingRight ? Vector2.right : Vector2.left;

        // Heitetään säde eteenpäin hain koosta riippuen
        RaycastHit2D hitObstacle = Physics2D.Raycast(transform.position, forwardVector, avoidanceDistance, obstacleMask);

        if (hitObstacle.collider != null)
        {
            // Jos edessä on seinä, hai muuttaa suuntaansa pystysuunnassa väistääkseen sen
            // hitObstacle.normal.y kertoo kumpaan suuntaan seinä "työntää" haita pois
            float avoidanceY = hitObstacle.normal.y != 0 ? Mathf.Sign(hitObstacle.normal.y) : 1f;
            moveDirection += new Vector2(0, avoidanceY * avoidanceForce);
            moveDirection.Normalize();
        }

        // Liikutetaan haita muokatun suunnan mukaan
        transform.Translate(moveDirection * patrolSpeed * Time.deltaTime, Space.World);

        HandleFacingAndRotation(moveDirection);

        // Kun ollaan tarpeeksi lähellä reittipistettä, vaihdetaan seuraavaan
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    void ChaseBehavior()
    {
        if (playerTransform == null) return;

        // Jahdassa hai ui suoraviivaisemmin kohti pelaajaa (horror-pelin henkeen kuuluu, että se puskee lujaa päälle!)
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
        canSeePlayerThisFrame = false;

        if (distanceToPlayer <= visionRange)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector2 forwardDirection = facingRight ? Vector2.right : Vector2.left;

            float angleToPlayer = Vector2.Angle(forwardDirection, directionToPlayer);

            if (angleToPlayer < visionAngle / 2f)
            {
                // TEHOSTETTU NÄKÖ: Ammutaan säde, joka hakee KAIKKI matkalla olevat asiat
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, directionToPlayer, distanceToPlayer, obstacleMask | playerMask);

                // Käydään läpi mihin osuttiin järjestyksessä (lähimmästä kauimpaan)
                foreach (var hit in hits)
                {
                    // Ignoroidaan hain oma collider, jottei se tuki näköä
                    if (hit.collider == sharkCollider) continue;

                    // Jos matkalla ensimmäisenä (hain jälkeen) on seinä/este, näköyhteys katkeaa
                    if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0)
                    {
                        break;
                    }

                    // Jos matkalla ensimmäisenä on pelaaja, hai näkee sen satavarmasti!
                    if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
                    {
                        canSeePlayerThisFrame = true;
                        break;
                    }
                }
            }
        }

        // Muisti/ajastinlogiikka pysyy samana
        if (canSeePlayerThisFrame)
        {
            lostTargetTimer = lostTargetDelay;
            if (currentState != SharkState.Chasing) OnPlayerSpotted();
        }
        else
        {
            if (currentState == SharkState.Chasing)
            {
                lostTargetTimer -= Time.deltaTime;
                if (lostTargetTimer <= 0f) OnPlayerLost();
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

        Vector3 forward = facingRight ? Vector3.right : Vector3.left;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2f) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2f) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + forward * avoidanceDistance);
    }
}