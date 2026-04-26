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

    private NavMeshAgent Agent;
    private Animator anim;
    private bool isAttacking = false;

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
            }
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
            }
        }
        else
        {
            Patrol();
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

        yield return new WaitForSeconds(attackInterval);

        Agent.isStopped = false;
        isAttacking = false;
    }
}
