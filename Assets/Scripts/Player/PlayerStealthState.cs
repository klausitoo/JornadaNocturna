using UnityEngine;

public class PlayerStealthState : MonoBehaviour
{
    public static PlayerStealthState Instance;

    [Header("Estado del jugador")]
    public bool IsRunning { get; private set; }
    public bool IsFlashlightOn { get; private set; }
    public bool IsHiding { get; private set; }

    [Header("Inventario")]
    [SerializeField] private bool startWithFlashlight = false;
    public bool HasFlashlight { get; private set; }

    [Header("Correr")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    private void Awake()
    {
        Instance = this;
        Debug.Log("PlayerStealthState activo en: " + gameObject.name);
    }

    private void Start()
    {
        HasFlashlight = startWithFlashlight;
        IsFlashlightOn = false;

        Debug.Log("Tiene linterna al empezar: " + HasFlashlight);
    }

    private void Update()
    {
        IsRunning = Input.GetKey(runKey);
    }

    public void PickUpFlashlight()
    {
        if (HasFlashlight)
        {
            Debug.Log("Ya tenías la linterna.");
            return;
        }

        HasFlashlight = true;
        IsFlashlightOn = false;

        Debug.Log("LINERNA AGARRADA. HasFlashlight ahora es: " + HasFlashlight);
    }

    public void SetFlashlightState(bool value)
    {
        if (!HasFlashlight && value)
        {
            IsFlashlightOn = false;
            Debug.Log("No tenés la linterna todavía.");
            return;
        }

        IsFlashlightOn = value;
        Debug.Log("Estado de linterna para enemigo: " + IsFlashlightOn);
    }

    public void SetHiding(bool value)
    {
        IsHiding = value;
    }
}