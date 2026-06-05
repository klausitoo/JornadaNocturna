using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientSoundTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] ambientClips;
    [SerializeField] private float delayBetweenClips = 1.5f;

    private AudioSource audioSource;
    private bool isPlaying = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlaying)
        {
            isPlaying = true;
            StartCoroutine(PlayAmbientPlaylist());
        }
    }

    private IEnumerator PlayAmbientPlaylist()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, ambientClips.Length);
            AudioClip clipToPlay = ambientClips[randomIndex];

            audioSource.clip = clipToPlay;
            audioSource.Play();

            yield return new WaitForSeconds(clipToPlay.length + delayBetweenClips);
        }
    }
}