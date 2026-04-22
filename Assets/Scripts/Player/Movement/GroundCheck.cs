using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private PlayerMovement movementScript;

    private void Update()
    {
        bool grounded = Physics.CheckSphere(transform.position, 0.2f, groundLayer);
        movementScript.SetIsGroundedValue(grounded);
    }

}
