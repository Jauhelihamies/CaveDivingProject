using UnityEngine;
using System.Collections;

public class SharkAI : MonoBehaviour
{
    private SharkAudioManager sharkAudio;
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

    [Tooltip("Kuinka usein näkökenttä tarkistetaan sekunneissa. Esim. 0.1f = 10 kertaa sekunnissa.")]
    public float visionTickRate = 0.1f;

    [Header("Obstacle Avoidance")]
    [Tooltip("Kuinka kauas eteenpäin hai katsoo seinien varalta.")]
    public float avoidanceDistance = 2f;
    [Tooltip("Kuinka voimakkaasti hai väistää seinää ylös/alas.")]
    public float avoidanceForce = 3f;

    private Transform playerTransform;
    private bool facingRight = false;
    private PlayerMovement cachedPlayerScript;
    private Collider2D sharkCollider;

    [Header("Lost Delay Settings")]
    [Tooltip("Kuinka monta sekuntia hai jatkaa jahtaamista sen jälkeen, kun pelaaja katosi näkyvistä.")]
    public float lostTargetDelay = 2.5f;
    private float lostTargetTimer = 0f;
    private bool canSeePlayerThisFrame = false;

    // Välimuistitetut muuttujat optimointia varten
    private int combinedMask;
    private float visionRangeSqr;
    private float patrolDistanceCheckSqr = 0.5f * 0.5f;
    private WaitForSeconds visionWait;
    private Transform myTransform;

    void Start()
    {
        myTransform = transform;

        sharkAudio = GetComponent<SharkAudioManager>();
        if (sharkAudio == null) sharkAudio = GetComponentInChildren<SharkAudioManager>();

        // KORJAUS 1: Pakotetaan audio latautumaan muistiin heti pelin alussa (Warm-up)
        // Jos SharkAudioManagerissasi on AudioSource, tämä valmistelee sen valmiiksi puskuriin.
        if (sharkAudio != null)
        {
            AudioSource source = sharkAudio.GetComponent<AudioSource>();
            if (source == null) source = sharkAudio.GetComponentInChildren<AudioSource>();
            if (source != null && source.clip != null)
            {
                source.clip.LoadAudioData(); // Lataa äänen datan RAM-muistiin nyt, eikä vasta kun musiikki alkaa!
            }
        }

        sharkCollider = GetComponent<Collider2D>();

        combinedMask = obstacleMask | playerMask;
        visionRangeSqr = visionRange * visionRange;
        visionWait = new WaitForSeconds(visionTickRate);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            cachedPlayerScript = playerTransform.GetComponent<PlayerMovement>();
            if (cachedPlayerScript == null) cachedPlayerScript = playerTransform.GetComponentInChildren<PlayerMovement>();
            if (cachedPlayerScript == null) cachedPlayerScript = playerTransform.GetComponentInParent<PlayerMovement>();
        }

        StartCoroutine(VisionRoutine());
    }

    void Update()
    {
        switch (currentState)
        {
            case SharkState.Patrolling:
                PatrolBehavior();
                break;
            case SharkState.Chasing:
                ChaseBehavior();
                break;
        }

        if (currentState == SharkState.Chasing && !canSeePlayerThisFrame)
        {
            lostTargetTimer -= Time.deltaTime;
            if (lostTargetTimer <= 0f)
            {
                OnPlayerLost();
            }
        }
    }

    IEnumerator VisionRoutine()
    {
        // KORJAUS 2: Hajautetaan useamman hain herääminen eri frameille, jos haita on monta
        yield return new WaitForSeconds(Random.Range(0f, visionTickRate));

        while (true)
        {
            CheckForPlayer();
            yield return visionWait;
        }
    }

    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        Vector3 currentPos = myTransform.position;
        Vector2 offsetToPlayer = playerTransform.position - currentPos;
        float sqrDistance = offsetToPlayer.sqrMagnitude;

        canSeePlayerThisFrame = false;

        if (sqrDistance <= visionRangeSqr)
        {
            Vector2 forwardDirection = facingRight ? Vector2.right : Vector2.left;
            float angleToPlayer = Vector2.Angle(forwardDirection, offsetToPlayer);

            if (angleToPlayer < visionAngle / 2f)
            {
                float distance = Mathf.Sqrt(sqrDistance);
                RaycastHit2D hit = Physics2D.Raycast(currentPos, offsetToPlayer, distance, combinedMask);

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
    }

    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector3 currentPos = myTransform.position;

        Vector2 offset = targetPoint.position - currentPos;
        float sqrDist = offset.sqrMagnitude;
        Vector2 moveDirection = offset.normalized;

        Vector2 forwardVector = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hitObstacle = Physics2D.Raycast(currentPos, forwardVector, avoidanceDistance, obstacleMask);

        if (hitObstacle.collider != null)
        {
            float avoidanceY = hitObstacle.normal.y != 0 ? Mathf.Sign(hitObstacle.normal.y) : 1f;
            moveDirection += new Vector2(0, avoidanceY * avoidanceForce);
            moveDirection.Normalize();
        }

        myTransform.Translate(moveDirection * patrolSpeed * Time.deltaTime, Space.World);
        HandleFacingAndRotation(moveDirection);

        if (sqrDist < patrolDistanceCheckSqr)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    void ChaseBehavior()
    {
        if (playerTransform == null) return;

        Vector3 currentPos = myTransform.position;
        Vector3 playerPos = playerTransform.position;

        myTransform.position = Vector2.MoveTowards(currentPos, playerPos, chaseSpeed * Time.deltaTime);

        Vector2 directionToPlayer = (playerPos - currentPos).normalized;
        HandleFacingAndRotation(directionToPlayer);
    }

    void HandleFacingAndRotation(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        Vector3 scale = myTransform.localScale;
        bool changeScale = false;

        if (direction.x > 0.1f && !facingRight)
        {
            facingRight = true;
            scale.x = -Mathf.Abs(scale.x);
            changeScale = true;
        }
        else if (direction.x < -0.1f && facingRight)
        {
            facingRight = false;
            scale.x = Mathf.Abs(scale.x);
            changeScale = true;
        }

        if (changeScale)
        {
            myTransform.localScale = scale;
        }

        myTransform.rotation = Quaternion.identity;
    }

    void OnPlayerSpotted()
    {
        currentState = SharkState.Chasing;
        cachedPlayerScript?.PlayerIsChased();
        sharkAudio?.StartChaseMusic();
    }

    void OnPlayerLost()
    {
        currentState = SharkState.Patrolling;
        myTransform.rotation = Quaternion.identity;
        cachedPlayerScript?.ChaseIsOver();
        sharkAudio?.StopChaseMusic();
    }
}
