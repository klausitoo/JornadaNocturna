using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MovingElevator : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin noiseComponent;

    [Header("Audio & Animation")]
    public Animator elevatorAnimator;
    public AudioSource movementAudioSource;
    public AudioSource doorAudioSource;

    [Header("Elevator Settings")]
    public bool isElevatorMoving = false;

    [Tooltip("Intensity")]
    public float movingAmplitude = 0.3f;
    public float transitionSpeed = 5f;

    void Start()
    {
        GetCinemachineData();
        StartElevatorSequence();
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

        float targetAmplitude = isElevatorMoving ? movingAmplitude : 0f;

        noiseComponent.AmplitudeGain = Mathf.Lerp(noiseComponent.AmplitudeGain, targetAmplitude, Time.deltaTime * transitionSpeed);
    }

    public void StartElevatorSequence()
    {
        if (!isElevatorMoving)
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
    }
}