using UnityEngine;
using UnityEngine.InputSystem;

public class FPSMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 7f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private GameObject interactionText; 
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference interactAction;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Rigidbody rb;
    
    private Vector2 moveInput;
    private bool isRunning;
    private bool isGrounded;
    private IInteractable currentInteractable; 
    private float lastInteractionTime;
    private float interactionCooldown = 0.5f; 
    
    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;
        if (interactionPoint == null) interactionPoint = cameraTransform;
        
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
            moveAction.action.Enable();
        }
        
        if (runAction != null)
        {
            runAction.action.performed += OnRun;
            runAction.action.canceled += OnRun;
            runAction.action.Enable();
        }
        
        if (jumpAction != null)
        {
            jumpAction.action.performed += OnJump;
            jumpAction.action.Enable();
        }
        
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }
    
    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled -= OnMove;
            moveAction.action.Disable();
        }
        
        if (runAction != null)
        {
            runAction.action.performed -= OnRun;
            runAction.action.canceled -= OnRun;
            runAction.action.Disable();
        }
        
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
            jumpAction.action.Disable();
        }
        
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        
        if (Time.time - lastInteractionTime < interactionCooldown) return;
        
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
            lastInteractionTime = Time.time;
            Debug.Log("Interactuaste con un objeto");
        }
    }
    
    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        DetectInteractable();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        Vector3 moveDirection = (forward * moveInput.y) + (right * moveInput.x);
        
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();
        
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }
    
    private void DetectInteractable()
    {
        Ray ray = new Ray(interactionPoint.position, interactionPoint.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    
                    if (interactionText != null)
                    {
                        interactionText.SetActive(true);
                        // Si el texto tiene componente Text o TMP, puedes actualizarlo
                        // interactionText.GetComponent<Text>().text = "Presiona E para interactuar";
                    }
                    Debug.Log("Mirando objeto interactuable");
                }
                return;
            }
        }
        
        
        if (currentInteractable != null)
        {
            currentInteractable = null;
            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
            Debug.Log("Dejaste de mirar objeto interactuable");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
        
        if (interactionPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(interactionPoint.position, interactionPoint.forward * interactionRange);
        }
    }
}