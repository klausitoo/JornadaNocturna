using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [Header("Linterna")]
    [SerializeField] private Light lightComponent;

    [Header("Input")]
    [SerializeField] private InputActionReference toggleAction;

    [Header("Estado del jugador")]
    [SerializeField] private PlayerStealthState playerStealth;

    [Header("Pickup")]
    [SerializeField] private bool giveFlashlightWhenThisObjectActivates = true;

    private void Awake()
    {
        if (lightComponent == null)
        {
            lightComponent = GetComponentInChildren<Light>(true);
        }

        if (playerStealth == null)
        {
            playerStealth = GetComponentInParent<PlayerStealthState>();
        }

        if (playerStealth == null)
        {
            playerStealth = PlayerStealthState.Instance;
        }

        ForceLight(false);

        Debug.Log("FlashlightController activo en: " + gameObject.name);
    }

    private void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
        }

        if (giveFlashlightWhenThisObjectActivates)
        {
            if (playerStealth == null)
            {
                playerStealth = PlayerStealthState.Instance;
            }

            if (playerStealth != null)
            {
                playerStealth.PickUpFlashlight();
                ForceLight(false);
                playerStealth.SetFlashlightState(false);
            }
            else
            {
                Debug.LogError("No se encontró PlayerStealthState al activar la linterna.");
            }
        }
    }

    private void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Disable();
        }
    }

    private void Update()
    {
        if (toggleAction != null && toggleAction.action.WasPressedThisFrame())
        {
            Debug.Log("Input de toggle de linterna detectado.");
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        if (playerStealth == null)
        {
            Debug.LogError("FlashlightController no encontró PlayerStealthState.");
            return;
        }

        if (!playerStealth.HasFlashlight)
        {
            ForceLight(false);
            playerStealth.SetFlashlightState(false);
            Debug.Log("No podés prender la linterna porque todavía no la agarraste.");
            return;
        }

        bool newState = !lightComponent.enabled;

        ForceLight(newState);
        playerStealth.SetFlashlightState(newState);

        Debug.Log("Linterna visual prendida: " + newState);
    }

    private void ForceLight(bool value)
    {
        if (lightComponent != null)
        {
            lightComponent.enabled = value;
        }
        else
        {
            Debug.LogError("No hay Light asignada en FlashlightController.");
        }
    }
}