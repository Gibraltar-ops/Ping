using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI;

public enum MonsterState { Idle, Patrolling, Investigating, Chasing }
public class MonsterAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private MonsterAnimationManager animManager;

    private ChaserAudioManager audioManager;
    private ScreenshakeScript screenShake;
    private PlayerStatus playerStatus;
    private Animator anim;

    [Header("UI")]
    [SerializeField] private Image suspicionBar;

    [Header("State Management")]
    [SerializeField] private MonsterState currentState = MonsterState.Patrolling;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Patrol settings")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float waitTimeAtPoint = 8f;
    private float waitTimer;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;
    private bool isWaiting;

    [Header("Detection")]

    public bool isAwake = false;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float investigateSpeed = 4.5f;
    [SerializeField] private float suspicionLevel = 0f;
    [SerializeField] private float suspicionDecay = 0.5f;
    [SerializeField] private float detectionThreshold = 3.5f;
    [SerializeField] private float vicinityThreshold = 5f;
    [SerializeField] private float vicinityDetectionRate = 0.5f;
    [SerializeField] private float investigateThreshold = 1.5f;
    [SerializeField] private float sonarReactionChance = 0.6f;
    [SerializeField] private float maxSonarDistance = 100f;
    [SerializeField] private float sonarSuspicionStrength = 1.5f;

    private bool hasCaughtPlayer = false;
    public bool isCharging => currentState == MonsterState.Chasing;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animManager == null) animManager = GetComponent<MonsterAnimationManager>();
        audioManager = GetComponent<ChaserAudioManager>();
        screenShake = GetComponent<ScreenshakeScript>();

        if (playerTransform != null)
            playerStatus = playerTransform.GetComponent<PlayerStatus>();

        if (suspicionBar != null) suspicionBar.fillAmount = 0;
    }

    private void Update()
    {
        if (hasCaughtPlayer || playerTransform == null) return;
        HandleSuspicion();
        UpdateBehaviourState();
        UpdateAnimationParameters();
        UpdateSuspicionUI();
    }

    private void HandleSuspicion()
    {
        if (currentState != MonsterState.Chasing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            bool playerInVicinity = distanceToPlayer <= vicinityThreshold;

            if (isAwake && playerInVicinity && (playerStatus == null || !playerStatus.isHidden))
            {
                suspicionLevel += vicinityDetectionRate * Time.deltaTime;
            }
            else if (suspicionLevel > 0)
            {
                suspicionLevel -= suspicionDecay * Time.deltaTime;
            }

            suspicionLevel = Mathf.Clamp(suspicionLevel, 0, detectionThreshold);

            if (suspicionLevel >= detectionThreshold)
            {
                TransitionToState(MonsterState.Chasing);
            }
        }
    }

    private void UpdateBehaviourState()
    {
        switch (currentState)
        {
            case MonsterState.Patrolling:
                navAgent.speed = walkSpeed;
                PerformPatrol();
                break;

            case MonsterState.Investigating:
                navAgent.speed = investigateSpeed;
                PerformInvestigation();
                break;

            case MonsterState.Chasing:
                break;
        }
    }

private void TransitionToState(MonsterState newState)
{
    if (currentState == newState) return;

    StopAllCoroutines();
    isWaiting = false;
    navAgent.isStopped = false;

    currentState = newState;

    if (newState == MonsterState.Chasing)
    {
        StartCoroutine(StartCharging());
    }
}

    public void RegisterSonarPing(Vector3 sonarPosition, float strength)
{
    if (!isAwake || hasCaughtPlayer) return;
    if (playerStatus != null && playerStatus.isHidden) return;

    if (currentState == MonsterState.Chasing)
    {
        navAgent.SetDestination(sonarPosition);
        return;
    }

    float distanceToPing = Vector3.Distance(transform.position, sonarPosition);
    if (distanceToPing > maxSonarDistance) return;

    float effectiveStrength = sonarSuspicionStrength * (1 - (distanceToPing / maxSonarDistance));
    suspicionLevel += effectiveStrength;

    if (suspicionLevel >= detectionThreshold)
    {
        TransitionToState(MonsterState.Chasing);
    }
    else if (suspicionLevel >= investigateThreshold)
    {
      
            currentPatrolPoint = sonarPosition;
            hasPatrolPoint = true;
            TransitionToState(MonsterState.Investigating);
    }
}
    
    private void PerformPatrol()
{
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0) isWaiting = false;
            return;
        }

        if (!hasPatrolPoint) FindPatrolPoint();

        navAgent.SetDestination(currentPatrolPoint);

        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            hasPatrolPoint = false;
        }
}

private void PerformInvestigation()
{
    if (HandleWaitTime())
    {
        if (!isWaiting) TransitionToState(MonsterState.Patrolling);
        return;
    }

    navAgent.SetDestination(currentPatrolPoint);

    if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
    {
        isWaiting = true;
        waitTimer= 5f;
    }
}

private bool HandleWaitTime()
{
    if (isWaiting)
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            isWaiting = false;
            return false;
        }
        return true;
    }
    return false;
}

private IEnumerator StartCharging()
{
    suspicionLevel = detectionThreshold;

    if (SettingsManager.Instance != null)
        SettingsManager.Instance.TriggerHapticFeedback();

    navAgent.isStopped = true;
    animManager.TriggerDetection();
    audioManager.PlayDetectionSound();
    yield return new WaitForSeconds(2f);

    navAgent.isStopped = false;
    navAgent.speed = runSpeed;

    float chaseTimer = 5f;
    while (chaseTimer > 0)
    {
        chaseTimer -= Time.deltaTime;
        navAgent.SetDestination(playerTransform.position);

        if (Vector3.Distance(transform.position, playerTransform.position) < 1.5f)
        {
            CatchPlayer();
            yield break;
        }
        
        suspicionLevel = Mathf.Lerp(suspicionLevel, 0f, Time.deltaTime * 0.5f);

        
        yield return null;
    }

    suspicionLevel = detectionThreshold - 1.5f;
    TransitionToState(MonsterState.Patrolling);
}

private void CatchPlayer()
{
    hasCaughtPlayer = true;
    navAgent.isStopped = true;
    if (screenShake != null) screenShake.TriggerShake(1f);
    GameManager.Instance.TriggerJumpscare(JumpscareType.Chaser, playerTransform.position);
}

private void FindPatrolPoint()
{
    Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
    randomDirection += transform.position;
    NavMeshHit hit;
    if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
    {
        currentPatrolPoint = hit.position;
        hasPatrolPoint = true;
    }
}
    
    private void UpdateAnimationParameters()
{
    bool moving = navAgent.velocity.magnitude > 0.1f;
    anim.SetBool("IsMoving", moving);
    anim.SetBool("IsRunning", currentState == MonsterState.Chasing);
}
    
    private void UpdateSuspicionUI()
    {
        if (suspicionBar != null)
            suspicionBar.fillAmount = suspicionLevel / detectionThreshold;
    }
}

