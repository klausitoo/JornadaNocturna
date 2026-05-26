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

        [Header("Bloqueo")]
        [SerializeField] private bool isLocked = true;
        [SerializeField] private KeyType requiredKey = KeyType.Ninguna;

        [Header("Audio")]
        [SerializeField] private AudioSource asource;
        [SerializeField] private AudioClip openDoor;
        [SerializeField] private AudioClip closeDoor;
        [SerializeField] private AudioClip lockedDoor;

        private void Start()
        {
            if (asource == null)
            {
                asource = GetComponent<AudioSource>();
            }

            float initialAngle = open ? doorOpenAngle : doorCloseAngle;
            transform.localRotation = Quaternion.Euler(0, initialAngle, 0);
        }

        private void Update()
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
            Debug.Log("Puerta tocada: " + gameObject.name, gameObject);
            Debug.Log("Llave requerida por esta puerta: " + requiredKey, gameObject);

            if (requiredKey == KeyType.Ninguna)
            {
                Debug.LogError("ERROR: esta puerta no tiene llave configurada.", gameObject);
                return;
            }

            if (isLocked)
            {
                if (PlayerKeyInventory.Instance == null)
                {
                    Debug.LogError("ERROR: el Player no tiene PlayerKeyInventory.");
                    return;
                }

                bool tieneLlaveCorrecta = PlayerKeyInventory.Instance.HasKey(requiredKey);

                Debug.Log("¿El jugador tiene la llave " + requiredKey + "? " + tieneLlaveCorrecta, gameObject);

                if (!tieneLlaveCorrecta)
                {
                    Debug.Log("Puerta bloqueada. No tenés la llave correcta.", gameObject);
                    PlayLockedSound();
                    return;
                }

                isLocked = false;
                Debug.Log("Puerta desbloqueada con: " + requiredKey, gameObject);
            }

            OpenDoor();
        }

        private void OpenDoor()
        {
            open = !open;

            if (asource != null)
            {
                asource.clip = open ? openDoor : closeDoor;

                if (asource.clip != null)
                {
                    asource.Play();
                }
            }
        }

        private void PlayLockedSound()
        {
            if (asource != null && lockedDoor != null)
            {
                asource.clip = lockedDoor;
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