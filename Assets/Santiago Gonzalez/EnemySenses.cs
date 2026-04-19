using UnityEngine;
using System.Collections;

public class EnemySenses : MonoBehaviour
{
    [Header("Visual Detection")]
    public float viewRadius = 15f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask targetMask;      // Layer del jugador
    public LayerMask obstacleMask;     // Obstáculos que bloquean visión
    
    [Header("Audio Detection")]
    public float hearingRadius = 20f;
    public float noiseMemoryTime = 3f; // Cuánto recuerda un ruido
    
    [Header("References")]
    public Transform player;
    public Transform eyesPoint; // Punto desde donde "mira" (ej: cabeza)
    
    // Estado del enemigo
    public bool canSeePlayer { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }
    private Vector3 lastHeardPosition;
    private float noiseTimer;
    
    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (eyesPoint == null)
            eyesPoint = transform;
            
        lastKnownPosition = transform.position;
    }
    
    void Update()
    {
        // Detección visual
        canSeePlayer = CanSeePlayer();
        
        if (canSeePlayer)
        {
            lastKnownPosition = player.position;
            noiseTimer = 0; // Resetea ruido si ve al player
        }
        
        // Detección auditiva (si hay ruido reciente y no ve al player)
        if (!canSeePlayer && noiseTimer > 0)
        {
            noiseTimer -= Time.deltaTime;
            if (noiseTimer <= 0)
                lastHeardPosition = transform.position;
        }
    }
    
    bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - eyesPoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(eyesPoint.position, player.position);
        
        // Dentro del radio?
        if (distanceToPlayer > viewRadius) return false;
        
        // Dentro del ángulo de visión?
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2) return false;
        
        // Raycast sin obstáculos?
        RaycastHit hit;
        if (Physics.Raycast(eyesPoint.position, directionToPlayer, out hit, viewRadius, obstacleMask))
        {
            if (hit.transform != player)
                return false;
        }
        
        return true;
    }
    
    // Método público para detectar ruidos (llamar desde jugador o eventos)
    public void HearNoise(Vector3 noisePosition, float loudness = 1f)
    {
        float distance = Vector3.Distance(transform.position, noisePosition);
        float effectiveHearingRadius = hearingRadius * loudness;
        
        if (distance <= effectiveHearingRadius && !canSeePlayer)
        {
            lastHeardPosition = noisePosition;
            lastKnownPosition = noisePosition;
            noiseTimer = noiseMemoryTime;
        }
    }
    
    public Vector3 GetTargetPosition()
    {
        if (canSeePlayer)
            return player.position;
        else if (noiseTimer > 0)
            return lastHeardPosition;
        else
            return lastKnownPosition; // Último punto conocido
    }
    
    // Visualización en editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
        
        if (eyesPoint != null)
        {
            Vector3 fovLine1 = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewRadius;
            Vector3 fovLine2 = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewRadius;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(eyesPoint.position, fovLine1);
            Gizmos.DrawRay(eyesPoint.position, fovLine2);
        }
    }
}
