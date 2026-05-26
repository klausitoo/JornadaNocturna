using UnityEngine;

namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]
    public class Door : MonoBehaviour, IInteractable 
    {
        [Header("Configuración")]
        [SerializeField] private bool open = false;
        [SerializeField] private float smooth = 1.0f;
        [SerializeField] private float doorOpenAngle = -90.0f;
        [SerializeField] private float doorCloseAngle = 0.0f;

        [Header("Audio")]
        [SerializeField] private AudioSource asource;
        [SerializeField] private AudioClip openDoor, closeDoor;

        private bool isLocked = false;

        void Start()
        {
            if (asource == null) asource = GetComponent<AudioSource>();

            float initialAngle = open ? doorOpenAngle : doorCloseAngle;
            transform.localRotation = Quaternion.Euler(0, initialAngle, 0);
        }

        void Update()
        {
            float targetAngle = open ? doorOpenAngle : doorCloseAngle;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * 5 * smooth
            );
        }

        public void Interact()
        {
            OpenDoor();
        }

        public void OpenDoor()
        {
            open = !open;

            if (asource != null && openDoor != null && closeDoor != null)
            {
                asource.clip = open ? openDoor : closeDoor;
                asource.Play();
            }
        }
        public void ForceClose()
        {
            open = false;
            isLocked = true;
        }
    }
}