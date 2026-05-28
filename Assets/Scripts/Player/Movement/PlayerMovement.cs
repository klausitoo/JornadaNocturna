using UnityEditor.Rendering.Canvas.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 3.5f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Jump and Fall")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpForce = 7f;

    [Header("Crouching")]
    private const float standingHeight = 2f;
    private const float crouchingHeight = 1f;
    private const float crouchingCollCenter = -0.5f;
    private const float standingCollCenter = 0;
    private const float crouchTransitionSpeed = 10f;
    private float _currentHeight;

    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private CapsuleCollider coll;

    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _isCrouching;
    private float _targetHeight;

    private float maxStamina = 100f;
    private float currentStamina;
    private float staminaRegenRate = 20f;
    private float staminaRegenDelay = 2f;
    private float staminaDrainRate = 30f;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;


    private float staminaRegenDelayTimer;

    private bool canRun => currentStamina > 0;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _targetHeight = standingHeight;
        _currentHeight = standingHeight;
        coll.height = standingHeight;
        currentStamina = maxStamina;
        staminaRegenDelayTimer = staminaRegenDelay;
    }

    private void OnEnable()
    {
        moveAction.action.performed += StoreMovementInput;
        moveAction.action.canceled += StoreMovementInput;
        jumpAction.action.performed += Jump;
        sprintAction.action.performed += Sprint;
        sprintAction.action.canceled += Sprint;
        crouchAction.action.performed += Crouch;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= StoreMovementInput;
        moveAction.action.canceled -= StoreMovementInput;
        jumpAction.action.performed -= Jump;
        sprintAction.action.performed -= Sprint;
        sprintAction.action.canceled -= Sprint;
        crouchAction.action.performed -= Crouch;
    }

    private void Update()
    {
        CheckGround();
        //Debug.Log(currentStamina);
        //Debug.Log(_isRunning);
        UpdateRigidBodyDamping();
        HandleMovement();
        HandleStamina();
        HandleStepClimbing();
    }

    private void FixedUpdate()
    {
        HandleCrouchTransition();

    }

    private void StoreMovementInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            //rb.AddForce(new(0, jumpForce, 0), ForceMode.Impulse);
        }
    }
    
    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            (_currentHeight / 2f) + 0.2f,
            groundLayer
        );

        Debug.DrawRay(
            transform.position,
            Vector3.down * ((_currentHeight / 2f) + 0.2f),
            Color.green
        );
    }

    private void UpdateRigidBodyDamping()
    {
        if (rb.linearVelocity.y != 0)
        {
            rb.linearDamping = 0;
        }
        else
        {
            rb.linearDamping = 10;
        }
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        if (_isCrouching)
        {
            if (!CanStandUp())
            {
                return;
            }
            _targetHeight = standingHeight;
        }
        else
        {
            _targetHeight = crouchingHeight;
        }
        _isCrouching = !_isCrouching;
    }

    private bool CanStandUp()
    {
        Vector3 origin = transform.position + new Vector3(0, _currentHeight / 2, 0);

        Vector3[] rayOrigins = {
            origin,
            origin + Vector3.right / 2,
            origin + Vector3.left / 2,
            origin + Vector3.forward / 2,
            origin + Vector3.back / 2
        };

        foreach (Vector3 rayOrigin in rayOrigins)
        {
            if (Physics.Raycast(rayOrigin, Vector3.up, 0.5f))
            {
                return false;
            }
        }
        return true;
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        if (!canRun) return;

        _isRunning = context.performed;
    }

    private void HandleStamina()
    {
        if (_isRunning) return;

        if (staminaRegenDelayTimer > 0)
        {
            staminaRegenDelayTimer -= Time.deltaTime;
            if (staminaRegenDelayTimer < 0)
            {
                staminaRegenDelayTimer = 0;
            }
                
            return;
        }

        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }
    private void HandleMovement()
    {
        if (!canRun)
        {
            _isRunning = false;
        }

        float currentSpeed = _isCrouching ? crouchSpeed : _isRunning ? runSpeed : walkSpeed;

        if (_isRunning)
        {
            staminaRegenDelayTimer = staminaRegenDelay;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;
        forward.y = 0;
        right.y = 0;

       Vector3 direction = forward.normalized * _moveInput.y + right.normalized * _moveInput.x;

        if (_isGrounded)
        {
            Vector3 finalMove = currentSpeed * direction;
            finalMove.y = rb.linearVelocity.y;

            rb.linearVelocity = finalMove;
        }
        else
        {
            //Vector3 airForce = direction * 2f;
            //rb.AddForce(airForce, ForceMode.Acceleration);

            Vector3 horizontalVelocity = new(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float maxSpeed = _isRunning ? runSpeed : walkSpeed;
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                Vector3 clamped = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new(clamped.x, rb.linearVelocity.y, clamped.z);
            }
        }
    }

    private void HandleCrouchTransition()
    {
        if (Mathf.Abs(_currentHeight - _targetHeight) < 0.01f)
        {
            _currentHeight = _targetHeight;
            return;
        }

        float newHeight = Mathf.Lerp(_currentHeight, _targetHeight, crouchTransitionSpeed * Time.deltaTime);
        float currentCollCenter = coll.center.y;
        float targetCollCenter = _targetHeight == crouchingHeight ?  crouchingCollCenter : standingCollCenter;
        float newCollCenter = Mathf.Lerp(currentCollCenter, targetCollCenter, crouchTransitionSpeed * Time.deltaTime);
        _currentHeight = newHeight;
        coll.height = newHeight;
        coll.center = new(0, newCollCenter, 0);

        Vector3 cameraTargetPosition = cam.localPosition;
        cameraTargetPosition.y = _targetHeight / 2;
        cam.localPosition = Vector3.Lerp(cam.localPosition, cameraTargetPosition, crouchTransitionSpeed * Time.deltaTime);
    }

    private void HandleStepClimbing()
    {
        if (_moveInput == Vector2.zero) return;

        Vector3 topRayOrigin = transform.position - new Vector3(0, _currentHeight / 4f, 0);
        Vector3 bottomRayOrigin = transform.position - new Vector3(0, _currentHeight / 2.1f, 0);

        Vector3 forward = cam.forward;
        forward.y = 0;
        Debug.DrawRay(bottomRayOrigin, forward, Color.blue, 5);
        Debug.DrawRay(topRayOrigin, forward, Color.blue, 5);

        if (Physics.Raycast(bottomRayOrigin, forward, 0.5f))
        {
            Debug.Log("Bottom ray hit something");
            if (!Physics.Raycast(topRayOrigin, forward, 0.55f))
            {
                // Obtenemos con un Ray la altura de la superficie del siguiente escalon //
                Vector3 downwardsRayOrigin = topRayOrigin + forward * 0.3f;

                if (Physics.Raycast(downwardsRayOrigin, Vector3.down, out RaycastHit hit))
                {
                    float actualStepHeight = transform.position.y - _currentHeight / 2f;
                    float targetStepHeight = hit.point.y - actualStepHeight;

                    transform.position += new Vector3(forward.x * 0.1f, targetStepHeight, forward.z * 0.1f);
                }
            }
            else
            {
                Debug.Log("Top ray hit something");
            }
        }
    }

    public void SetIsGroundedValue(bool value)
    {
        _isGrounded = value;
    }
}
