using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInteraction : MonoBehaviour
{
    [SerializeField] float interactDistance = 3f;
    [SerializeField] GameObject textHint;
    [SerializeField] InputActionReference interactAction;

    private void OnEnable() => interactAction.action.Enable();
    private void OnDisable() => interactAction.action.Disable();

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDistance))
        {
            IInteractable interactable = hit.transform.GetComponent<IInteractable>();
            if (interactable == null) interactable = hit.transform.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                if (textHint != null) textHint.SetActive(true);

                // PRUEBA A: Usar el Input System (Tu forma actual)
                if (interactAction.action.WasPressedThisFrame())
                {
                    interactable.Interact();
                }
                return;
            }
        }

        if (textHint != null) textHint.SetActive(false);
    }
}