using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MovingElevator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CinemachineCamera cinemachineCamera;
    [SerializeField] Collider elevatorTrigger;
    private CinemachineBasicMultiChannelPerlin noiseComponent;

    [Header("Audio & Animation")]
    [SerializeField] Animator elevatorAnimator;
    [SerializeField] AudioSource movementAudioSource;
    [SerializeField] AudioSource doorAudioSource;

    [Header("Elevator Settings")]
    [SerializeField] bool isElevatorMoving = false;
    [SerializeField] bool isPlayerInside = false;

    [Tooltip("Intensity")]
    [SerializeField] float movingAmplitude = 0.02f;
    [SerializeField] float transitionSpeed = 5f;

    void Start()
    {
        GetCinemachineData();
    }

    void Update()
    {
        ElevatorMovingNoise();
    }

    void GetCinemachineData()
    {
        if (cinemachineCamera != null)
        {
            noiseComponent = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

            if (noiseComponent != null)
            {
                noiseComponent.AmplitudeGain = 0f;
            }
        }
    }

    void ElevatorMovingNoise()
    {
        if (noiseComponent == null) return;

        float targetAmplitude = (isElevatorMoving && isPlayerInside) ? movingAmplitude : 0f;

        noiseComponent.AmplitudeGain = Mathf.Lerp(noiseComponent.AmplitudeGain, targetAmplitude, Time.deltaTime * transitionSpeed);
    }

    public void StartElevatorSequence()
    {
        if (!isElevatorMoving && isPlayerInside)
        {
            StartCoroutine(ElevatorRoutine());
        }
    }

    private IEnumerator ElevatorRoutine()
    {
        isElevatorMoving = true;

        if (movementAudioSource != null && movementAudioSource.clip != null)
        {
            movementAudioSource.Play();
            yield return new WaitForSeconds(movementAudioSource.clip.length);
        }
        isElevatorMoving = false;

        yield return new WaitForSeconds(2f);

        if (elevatorAnimator != null)
        {
            elevatorAnimator.SetTrigger("Opening");
        }

        if (doorAudioSource != null)
        {
            doorAudioSource.Play();
        }

        elevatorTrigger.enabled = false;

    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("¡Un objeto tocó el ascensor!: " + other.gameObject.name + " | Su Tag es: " + other.tag);
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            StartElevatorSequence();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
}