using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Player")]
    public Transform Player;

    [Header("Ranges")]
    public float detectionRange = 10f;
    public float attackDistance = 3f;
    public float attackInterval = 2f;

    [Header("Damage")]
    public int damageAmount = 1; 
    public bool damageOnlyOncePerAttack = true; 
    private bool hasDamagedInThisAttack = false;

    private NavMeshAgent Agent;
    private Animator anim;
    private bool isAttacking = false;
    
    // Referencia al sistema de vida del jugador
    private HealthSystem playerLifeSystem;

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
                
                playerLifeSystem = playerObj.GetComponent<HealthSystem>();
            }
            else
            {
                Debug.LogError("No se encontró el objeto con tag Player");
            }
        }
        else
        {
            
            playerLifeSystem = Player.GetComponent<HealthSystem>();
        }
    }

    void Update()
    {
        if (Player == null) return;

        float distance = Vector3.Distance(transform.position, Player.position);
        
        if (distance <= detectionRange)
        {
            LookAtTarget(Player.position);
            
            if (distance <= attackDistance)
            {
                Agent.ResetPath();
                anim.SetBool("isWalking", false);

                if (!isAttacking)
                {
                    StartCoroutine(PlayAttackAnimation());
                }
            }
            else
            {
                Agent.isStopped = false;
                Agent.SetDestination(Player.position);
                anim.SetBool("isWalking", true);
                
                hasDamagedInThisAttack = false;
            }
        }
        else
        {
            Patrol();
            
            hasDamagedInThisAttack = false;
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Agent.ResetPath();
            anim.SetBool("isWalking", false);
            return;
        }

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

        anim.SetBool("isWalking", true);
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
        anim.SetTrigger("Attack");
        
        // Resetear flag de daño al comenzar el ataque
        hasDamagedInThisAttack = false;
        
        // Esperar un momento para aplicar el daño (cuando el golpe impacta)
      
        float damageDelay = 0.5f;
        yield return new WaitForSeconds(damageDelay);
        
        // Aplicar daño solo si el jugador sigue en rango
        if (!hasDamagedInThisAttack && damageOnlyOncePerAttack)
        {
            ApplyDamageToPlayer();
            hasDamagedInThisAttack = true;
        }

        yield return new WaitForSeconds(attackInterval - damageDelay);

        Agent.isStopped = false;
        isAttacking = false;
    }
    
   
    /// Aplica daño al jugador
  
    void ApplyDamageToPlayer()
    {
        if (playerLifeSystem != null && !playerLifeSystem.IsGameOver())
        {
            // Verificar que el jugador aún esté en rango de ataque
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
            if (distanceToPlayer <= attackDistance + 1f) // Pequeño margen
            {
                playerLifeSystem.TakeDamage();
                Debug.Log($"Enemigo atacó! Daño aplicado. Vidas restantes: {playerLifeSystem.GetCurrentLives()}");
            }
        }
        else if (playerLifeSystem == null)
        {
            Debug.LogWarning("No se encontró el componente FPSLifeSystem en el jugador");
        }
    }
    
  
    public void OnAttackHit()
    {
        if (isAttacking && damageOnlyOncePerAttack && !hasDamagedInThisAttack)
        {
            ApplyDamageToPlayer();
            hasDamagedInThisAttack = true;
        }
    }
    
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}