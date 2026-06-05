using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip concreteStepClip;
    private AudioSource audioSource;

    [Header("Step timings")]

    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    private float stepTimer = 0f;

    [Header("Player References")]
    [SerializeField] private Rigidbody playerRigidbody;

    [SerializeField] private bool isRunning = false;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);

        if (horizontalVelocity.magnitude > 0.1f)
        {
            // El temporizador avanza con el tiempo real
            stepTimer += Time.deltaTime;

            // Decidimos cuál es el límite de tiempo dependiendo de si corremos o caminamos
            float currentInterval = isRunning ? runStepInterval : walkStepInterval;

            // Si el temporizador supera el límite, reproducimos el paso
            if (stepTimer >= currentInterval)
            {
                PlayFootstep();
                stepTimer = 0f; // Reiniciamos el cronómetro
            }
        }
        else
        {
            // Si nos detenemos, reseteamos el cronómetro. 
            // Así, el primer paso al volver a arrancar sonará de inmediato.
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        // TRUCO DE DISEÑO SONORO: 
        // Variamos ligeramente el tono (pitch) y volumen en cada pisada.
        // Esto engaña al cerebro y evita que el sonido se sienta como una "metralleta" repetitiva.
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = Random.Range(0.8f, 1f);

        // Usamos PlayOneShot para que los audios se puedan solapar si ocurren muy rápido
        audioSource.PlayOneShot(concreteStepClip);
    }
}
