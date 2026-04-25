using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float stoppingDistance = 1.5f;
    public float patrolWaitTime = 2f;
    
    [Header("Patrol Points")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex;
    private float waitTimer;
    
    private NavMeshAgent agent;
    private EnemySenses senses;
    private Animator animator; // Opcional
    
    private enum State { Patrol, Investigate, Chase }
    private State currentState;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        senses = GetComponent<EnemySenses>();
        animator = GetComponent<Animator>();
        
        currentState = State.Patrol;
        agent.speed = patrolSpeed;
        
        if (patrolPoints.Length > 0)
            GoToNextPatrolPoint();
    }
    
    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Investigate:
                InvestigateUpdate();
                break;
            case State.Chase:
                ChaseUpdate();
                break;
        }
        
        // Transiciones de estado
        UpdateState();
        
        // Animaciones (opcional)
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);
    }
    
    void UpdateState()
    {
        if (senses.canSeePlayer)
        {
            currentState = State.Chase;
            return;
        }
        
        // Si escucha algo o tiene última posición conocida
        Vector3 target = senses.GetTargetPosition();
        if (Vector3.Distance(target, transform.position) > 1f && 
            currentState != State.Chase && 
            !senses.canSeePlayer)
        {
            if (Vector3.Distance(target, senses.lastKnownPosition) > 0.1f)
                currentState = State.Investigate;
        }
        
        // Si llega a la posición investigada y no ve nada, vuelve a patrullar
        if (currentState == State.Investigate && 
            agent.remainingDistance < stoppingDistance &&
            !senses.canSeePlayer)
        {
            currentState = State.Patrol;
            GoToNextPatrolPoint();
        }
    }
    
    void PatrolUpdate()
    {
        if (patrolPoints.Length == 0) return;
        
        if (agent.remainingDistance < stoppingDistance)
        {
            if (waitTimer <= 0)
            {
                GoToNextPatrolPoint();
                waitTimer = patrolWaitTime;
            }
            else
            {
                waitTimer -= Time.deltaTime;
            }
        }
    }
    
    void InvestigateUpdate()
    {
        Vector3 investigatePoint = senses.GetTargetPosition();
        agent.SetDestination(investigatePoint);
        agent.speed = patrolSpeed;
    }
    
    void ChaseUpdate()
    {
        agent.SetDestination(senses.player.position);
        agent.speed = chaseSpeed;
        
        if (agent.remainingDistance <= stoppingDistance)
        {
            // Aquí puedes atacar al jugador
            Debug.Log("Player caught!");
        }
    }
    
    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }
}

