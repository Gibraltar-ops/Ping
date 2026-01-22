using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI;
using NUnit.Framework;

public class MonsterAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private MonsterAnimationManager animManager;
    private ChaserAudioManager audioManager;
    private Animator anim;
    [Header("UI")]
    [SerializeField] private Image suspicionBar;

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
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float investigateSpeed = 4.5f;
    [SerializeField] private float suspicionLevel = 0f;
    [SerializeField] private float suspicionDecay = 0.5f;
    [SerializeField] private float detectionThreshold = 3.5f;

    [SerializeField] private float investigateThreshold = 1.5f; 
[SerializeField] private float sonarReactionChance = 0.6f;
    [SerializeField] private float maxSonarDistance = 100f;

    [Header("State Flags")]
    public bool isCharging;
    private bool isInvestigating;

    private bool hasCaughtPlayer = false;

/*     [Header("Chase settings")]
    [SerializeField] private float runSpeed = 6.5f;
    [SerializeField] private float visionRange = 5f;
    [SerializeField] private float engagementRange = 2f; */

    private bool isPlayerVisible;
    private bool isPlayerInRange;
    private bool wasPlayerVisibleLastFrame;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animManager == null) animManager = GetComponent<MonsterAnimationManager>();
        audioManager = GetComponent<ChaserAudioManager>();

        if (suspicionBar != null)
        {
            suspicionBar.fillAmount = 0;
        }
    }

    private void Update()
    {
        if (!isCharging && suspicionLevel > 0)
        {
            suspicionLevel -= suspicionDecay * Time.deltaTime;
            suspicionLevel = Mathf.Max(0, suspicionLevel);
        }
        UpdateBehaviourState();
        UpdateAnimationParameters();

        UpdateSuspicionUI();
    }

    private void UpdateSuspicionUI()
    {
        if (suspicionBar != null)
        {
            suspicionBar.fillAmount = suspicionLevel / detectionThreshold;
        }
    }

   public void RegisterSonarPing(Vector3 sonarPosition, float strength)
{
        if (isCharging || hasCaughtPlayer) return;
    
    float distanceToPing = Vector3.Distance(transform.position, sonarPosition);
        if (distanceToPing > maxSonarDistance) return;
    
    float effectiveStrength = strength * (1 - (distanceToPing / maxSonarDistance));
    suspicionLevel += effectiveStrength;
    if (suspicionLevel >= detectionThreshold)
    {
        StopAllCoroutines();
        StartCoroutine(StartCharging());
    }
    else if (suspicionLevel >= investigateThreshold)
    {
        if (Random.value < sonarReactionChance)
        {
            InvestigatePosition(sonarPosition);
        }
    }
}

    private void InvestigatePosition(Vector3 pos)
    {
        isInvestigating = true;
        isWaiting = false;
        navAgent.isStopped = false;
        navAgent.speed = investigateSpeed;
        navAgent.SetDestination(pos);
        hasPatrolPoint = true;
        currentPatrolPoint = pos;
    }
    private IEnumerator StartCharging()
    {
        isCharging = true;
        suspicionLevel = detectionThreshold;

        navAgent.isStopped = true;
        animManager.TriggerDetection();
        audioManager.PlayDetectionSound();
        yield return new WaitForSeconds(1.5f);
        navAgent.isStopped = false;
        navAgent.speed = runSpeed;
        navAgent.SetDestination(playerTransform.position);

        float chaseTimer = 5f;
        while (chaseTimer > 0)
        {
            navAgent.SetDestination(playerTransform.position);
            chaseTimer -= Time.deltaTime;
            if (Vector3.Distance(transform.position, playerTransform.position) < 1.5f)
        {
                hasCaughtPlayer = true;
                navAgent.isStopped = true;

                GameManager.Instance.EndGame();

                yield break;
            }

            yield return null;
        }

        isCharging = false;
        suspicionLevel = 0;
        navAgent.speed = walkSpeed;
    }
   private void UpdateBehaviourState()
    {
        if (isCharging)
        {
            return;
        }

        if (isInvestigating)
        {
            navAgent.speed = investigateSpeed;
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                if (!isWaiting)
                {
                    isWaiting = true;
                    waitTimer = 5f;
                }
            }
        }
        else
        {
            navAgent.speed = walkSpeed;
            PerformPatrol();
        }
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                hasPatrolPoint = false;
                if (isInvestigating)
                {
                    isInvestigating = false;
                }
            }
        }
    }

    private void UpdateAnimationParameters()
    {
  bool moving = navAgent.velocity.magnitude > 0.1f;
        anim.SetBool("IsMoving", moving);
        anim.SetBool("IsRunning", isCharging);
    }

    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);
        Vector3 potentialPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentialPoint, out hit, 2f, NavMesh.AllAreas))
        {
            currentPatrolPoint = hit.position;
            hasPatrolPoint = true;
        }
    }

    private void PerformPatrol()
    {
        if (isWaiting) return;

        if (!hasPatrolPoint)
        {
            FindPatrolPoint();
        }

        if (hasPatrolPoint)
        {
            navAgent.SetDestination(currentPatrolPoint);
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                isWaiting = true;
                waitTimer = waitTimeAtPoint;
            }
        }
    }

    private void PerformChase()
    {
        if (playerTransform != null)
        {
            navAgent.SetDestination(playerTransform.position);
        }
    }

    IEnumerator StopForDetection(float duration)
    {
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
        anim.SetBool("IsMoving", false);
        yield return new WaitForSeconds(duration);
        navAgent.isStopped = false;
    }
}