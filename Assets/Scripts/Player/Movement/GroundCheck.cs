using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position, 0.2f, groundLayer);

        movementScript.SetIsGroundedValue(isGrounded);
    }
}
