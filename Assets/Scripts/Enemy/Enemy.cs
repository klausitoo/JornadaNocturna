using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Search
    }

    [Header("Patrulla opcional")]
    public bool patrolWhileIdle = false;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Player")]
    public Transform Player;
    private PlayerStealthState playerStealth;

    [Header("Detección")]
    public float normalDetectionRange = 3f;
    public float runningDetectionRange = 10f;
    public float flashlightDetectionRange = 12f;
    public float loseInterestTime = 3f;

    [Header("Ataque")]
    public float attackDistance = 3f;
    public float attackInterval = 2f;

    [Header("Damage")]
    public int damageAmount = 1;
    public bool damageOnlyOncePerAttack = true;
    private bool hasDamagedInThisAttack = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip footstepClip;
    public float footstepInterval = 0.5f;
    private float stepTimer = 0f;

    private NavMeshAgent Agent;
    private Animator anim;
    private bool isAttacking = false;

    private HealthSystem playerLifeSystem;

    private EnemyState currentState = EnemyState.Idle;
    private Vector3 lastKnownPlayerPosition;
    private float timeWithoutSeeingPlayer = 0f;

    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        Agent.updateRotation = false;

        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                Player = playerObj.transform;
            }
            else
            {
                Debug.LogError("No se encontró el objeto con tag Player");
                return;
            }
        }

        playerLifeSystem = Player.GetComponent<HealthSystem>();
        playerStealth = Player.GetComponent<PlayerStealthState>();

        if (playerStealth == null)
        {
            playerStealth = Player.GetComponentInParent<PlayerStealthState>();
        }

        if (playerStealth == null)
        {
            playerStealth = Player.GetComponentInChildren<PlayerStealthState>();
        }

        if (playerStealth == null)
        {
            playerStealth = PlayerStealthState.Instance;
        }

        if (playerStealth == null)
        {
            Debug.LogError("El enemigo NO encontró PlayerStealthState.");
        }
        else
        {
            Debug.Log("El enemigo encontró PlayerStealthState correctamente.");
        }

        GoIdle();
    }

    void Update()
    {
 
        HandleFootsteps();

        if (Player == null) return;

        float distance = Vector3.Distance(transform.position, Player.position);

        // Si el jugador está escondido, el enemigo deja de seguirlo
        if (PlayerIsHiding())
        {
            LosePlayer();
            return;
        }

        bool detectedPlayer = CanDetectPlayer(distance);

        if (detectedPlayer)
        {
            lastKnownPlayerPosition = Player.position;
            timeWithoutSeeingPlayer = 0f;

            if (distance <= attackDistance)
            {
                AttackPlayer();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            HandleNotDetected();
        }
    }

    private void HandleFootsteps()
    {
        if (Agent != null && Agent.velocity.magnitude > 0.1f && !isAttacking)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= footstepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (audioSource != null && footstepClip != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.volume = Random.Range(0.8f, 1f);
            audioSource.PlayOneShot(footstepClip);
        }
    }

    private bool CanDetectPlayer(float distance)
    {
        float currentDetectionRange = normalDetectionRange;

        if (playerStealth != null)
        {
            if (playerStealth.IsRunning)
            {
                currentDetectionRange = Mathf.Max(currentDetectionRange, runningDetectionRange);
            }

            if (playerStealth.IsFlashlightOn)
            {
                currentDetectionRange = Mathf.Max(currentDetectionRange, flashlightDetectionRange);
                //Debug.Log("Linterna prendida detectada por el enemigo. Rango actual: " + currentDetectionRange);
            }
        }
        else
        {
            //Debug.LogError("playerStealth es NULL en Enemy.");
        }

        return distance <= currentDetectionRange;
    }

    private bool PlayerIsHiding()
    {
        return playerStealth != null && playerStealth.IsHiding;
    }

    private void ChasePlayer()
    {
        currentState = EnemyState.Chase;

        Agent.isStopped = false;
        Agent.SetDestination(Player.position);

        LookAtTarget(Player.position);

        if (anim != null)
        {
            anim.SetBool("isWalking", true);
        }

        hasDamagedInThisAttack = false;
    }

    private void AttackPlayer()
    {
        currentState = EnemyState.Attack;

        Agent.ResetPath();

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
        }

        LookAtTarget(Player.position);

        if (!isAttacking)
        {
            StartCoroutine(PlayAttackAnimation());
        }
    }

    private void HandleNotDetected()
    {
        if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        {
            currentState = EnemyState.Search;
        }

        if (currentState == EnemyState.Search)
        {
            timeWithoutSeeingPlayer += Time.deltaTime;

            Agent.isStopped = false;
            Agent.SetDestination(lastKnownPlayerPosition);

            if (anim != null)
            {
                anim.SetBool("isWalking", true);
            }

            LookAtTarget(lastKnownPlayerPosition);

            if (timeWithoutSeeingPlayer >= loseInterestTime)
            {
                GoIdle();
            }
        }
        else
        {
            if (patrolWhileIdle)
            {
                Patrol();
            }
            else
            {
                GoIdle();
            }
        }

        hasDamagedInThisAttack = false;
    }

    private void LosePlayer()
    {
        StopAllCoroutines();

        isAttacking = false;
        hasDamagedInThisAttack = false;
        timeWithoutSeeingPlayer = 0f;

        GoIdle();

        Debug.Log("El enemigo perdió al jugador porque se escondió.");
    }

    private void GoIdle()
    {
        currentState = EnemyState.Idle;

        if (Agent != null)
        {
            Agent.ResetPath();
            Agent.isStopped = true;
        }

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            GoIdle();
            return;
        }

        currentState = EnemyState.Idle;

        Agent.isStopped = false;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Agent.SetDestination(targetPoint.position);
        LookAtTarget(targetPoint.position);

        if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance + 0.2f)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = 0;
            }
        }

        if (anim != null)
        {
            anim.SetBool("isWalking", true);
        }
    }

    void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    IEnumerator PlayAttackAnimation()
    {
        isAttacking = true;
        Agent.isStopped = true;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        hasDamagedInThisAttack = false;

        float damageDelay = 0.5f;
        yield return new WaitForSeconds(damageDelay);

        if (!PlayerIsHiding())
        {
            if (!hasDamagedInThisAttack && damageOnlyOncePerAttack)
            {
                ApplyDamageToPlayer();
                hasDamagedInThisAttack = true;
            }
        }

        yield return new WaitForSeconds(attackInterval - damageDelay);

        Agent.isStopped = false;
        isAttacking = false;
    }

    void ApplyDamageToPlayer()
    {
        if (playerLifeSystem != null && !playerLifeSystem.IsGameOver())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

            if (distanceToPlayer <= attackDistance + 1f && !PlayerIsHiding())
            {
                playerLifeSystem.TakeDamage();
                Debug.Log($"Enemigo atacó. Vidas restantes: {playerLifeSystem.GetCurrentLives()}");
            }
        }
        else if (playerLifeSystem == null)
        {
            Debug.LogWarning("No se encontró el componente Sistemadevida en el jugador");
        }
    }

    public void OnAttackHit()
    {
        if (isAttacking && damageOnlyOncePerAttack && !hasDamagedInThisAttack)
        {
            if (!PlayerIsHiding())
            {
                ApplyDamageToPlayer();
                hasDamagedInThisAttack = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, normalDetectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, runningDetectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, flashlightDetectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}