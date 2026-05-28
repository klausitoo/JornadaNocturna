using UnityEngine;

public class HideZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStealthState stealth = other.GetComponent<PlayerStealthState>();

            if (stealth != null)
            {
                stealth.SetHiding(true);
                Debug.Log("Jugador escondido");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStealthState stealth = other.GetComponent<PlayerStealthState>();

            if (stealth != null)
            {
                stealth.SetHiding(false);
                Debug.Log("Jugador salió del escondite");
            }
        }
    }
}