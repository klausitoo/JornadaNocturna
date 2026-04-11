using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerMovement movementScript;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ground"))
        {
            movementScript.SetIsGroundedValue(false);
        }
        else
        {
            movementScript.SetIsGroundedValue(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            movementScript.SetIsGroundedValue(false);
        }
    }
}
