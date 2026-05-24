using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementTest : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 3.5f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Jump and Fall")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;

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
    [SerializeField] private CapsuleCollider coll;

    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _isCrouching;
    private float _targetHeight;

    private void Awake()
    {
        if (coll == null) coll = GetComponent<CapsuleCollider>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        
        // IMPORTANTE: Congelar rotación para FPS
        rb.freezeRotation = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _targetHeight = standingHeight;
        _currentHeight = standingHeight;
        coll.height = standingHeight;
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
        // Detección de suelo
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        
        UpdateRigidBodyDamping();
        HandleMovement();
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
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }

    private void UpdateRigidBodyDamping()
    {
        // Reducir drag cuando está en el aire
        rb.linearDamping = _isGrounded ? 10f : 0f;
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        if (_isCrouching)
        {
            if (!CanStandUp()) return;
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
        return !Physics.Raycast(origin, Vector3.up, 0.5f);
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        _isRunning = context.performed;
        
        // Evitar correr mientras se agacha
        if (_isRunning && _isCrouching)
        {
            _isRunning = false;
        }
    }

    private void HandleMovement()
{
    float currentSpeed = _isCrouching ? crouchSpeed : _isRunning ? runSpeed : walkSpeed;

    // FORZAR obtener la rotación de la cámara de otra manera
    float cameraYaw = cam.rotation.eulerAngles.y;
    
    // Alternativa: usar transform.forward de la cámara directamente
    Vector3 forward = cam.transform.forward;
    Vector3 right = cam.transform.right;
    
    // Eliminar componente Y
    forward.y = 0;
    right.y = 0;
    
    // Normalizar
    forward.Normalize();
    right.Normalize();
    
    // Calcular dirección de movimiento
    Vector3 moveDirection = (forward * _moveInput.y) + (right * _moveInput.x);
    
    // Normalizar diagonal
    if (moveDirection.magnitude > 1f)
        moveDirection.Normalize();
    
    // DEBUG: Mostrar dirección calculada
    if (_moveInput != Vector2.zero)
    {
        Debug.Log($"Input: ({_moveInput.x}, {_moveInput.y})");
        Debug.Log($"Camera Forward: {forward}");
        Debug.Log($"Camera Right: {right}");
        Debug.Log($"Move Direction: {moveDirection}");
        Debug.Log($"Camera Yaw: {cameraYaw}");
    }
    
    if (_isGrounded)
    {
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }
    else
    {
        Vector3 airForce = moveDirection * 2f;
        rb.AddForce(airForce, ForceMode.Acceleration);
        
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float maxSpeed = _isRunning ? runSpeed : walkSpeed;
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }
}

    private void HandleCrouchTransition()
    {
        if (Mathf.Abs(_currentHeight - _targetHeight) < 0.01f)
        {
            _currentHeight = _targetHeight;
            coll.height = _currentHeight;
            coll.center = new Vector3(0, _targetHeight == crouchingHeight ? crouchingCollCenter : standingCollCenter, 0);
            return;
        }

        float newHeight = Mathf.Lerp(_currentHeight, _targetHeight, crouchTransitionSpeed * Time.deltaTime);
        float targetCollCenter = _targetHeight == crouchingHeight ? crouchingCollCenter : standingCollCenter;
        float newCollCenter = Mathf.Lerp(coll.center.y, targetCollCenter, crouchTransitionSpeed * Time.deltaTime);
        
        _currentHeight = newHeight;
        coll.height = newHeight;
        coll.center = new Vector3(0, newCollCenter, 0);

        // Ajustar cámara
        Vector3 cameraTargetPosition = cam.localPosition;
        cameraTargetPosition.y = _targetHeight - 0.2f;
        cam.localPosition = Vector3.Lerp(cam.localPosition, cameraTargetPosition, crouchTransitionSpeed * Time.deltaTime);
    }

    private void HandleStepClimbing()
    {
        if (_moveInput == Vector2.zero) return;

        // Obtener dirección de movimiento
        float cameraYaw = cam.eulerAngles.y;
        Vector3 inputDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
        Vector3 moveDirection = Quaternion.Euler(0, cameraYaw, 0) * inputDirection;
        moveDirection.Normalize();
        
        Vector3 bottomRayOrigin = transform.position - new Vector3(0, _currentHeight / 2.1f, 0);
        Vector3 topRayOrigin = transform.position - new Vector3(0, _currentHeight / 4f, 0);

        if (Physics.Raycast(bottomRayOrigin, moveDirection, 0.3f) && 
            !Physics.Raycast(topRayOrigin, moveDirection, 0.4f))
        {
            Vector3 stepPosition = topRayOrigin + moveDirection * 0.2f;
            if (Physics.Raycast(stepPosition, Vector3.down, out RaycastHit hit, 0.5f))
            {
                float stepHeight = hit.point.y - (transform.position.y - _currentHeight / 2f);
                if (stepHeight > 0 && stepHeight < 0.3f)
                {
                    transform.position += new Vector3(0, stepHeight, 0);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}