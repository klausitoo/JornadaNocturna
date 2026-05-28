using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    private bool wasPickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (wasPickedUp) return;

        Debug.Log(
            "La linterna tocó a: " + other.gameObject.name +
            " | Tag: " + other.tag +
            " | Root: " + other.transform.root.name
        );

        PlayerStealthState stealth = other.GetComponentInParent<PlayerStealthState>();

        bool isPlayer =
            other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player") ||
            stealth != null;

        if (!isPlayer)
        {
            Debug.Log("No es el Player, no agarro la linterna.");
            return;
        }

        if (stealth == null)
        {
            stealth = PlayerStealthState.Instance;
        }

        if (stealth == null)
        {
            Debug.LogError("No se encontró PlayerStealthState al agarrar la linterna.");
            return;
        }

        wasPickedUp = true;

        stealth.PickUpFlashlight();

        Debug.Log("LINERNA AGARRADA CORRECTAMENTE.");

        Destroy(gameObject);
    }
}