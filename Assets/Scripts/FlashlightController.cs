using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] Light lightComponent;
    [SerializeField] InputActionReference toggleAction;

    private void OnEnable()
    {
        if (toggleAction != null) toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        if (toggleAction != null) toggleAction.action.Disable();
    }

    void Update()
    {
        ToggleFlashlight();
    }

    void ToggleFlashlight()
    {
        if (toggleAction.action.WasPressedThisFrame())
        {
            if (lightComponent != null)
            {
                lightComponent.enabled = !lightComponent.enabled;
            }
        }
    }
}