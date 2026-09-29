using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum MovementState { walking, sprinting, crouching, sliding, airborne }

    [Header("Guns")]
    public Camera playerCam;
    public Weapon rightWeapon;
    public Weapon leftWeapon;
    private InputAction rightFireInput;
    private InputAction leftFireInput;

    [Header("Movement")]
    public Transform orientation;
    public float baseMoveSpeed;
    public float baseSprintSpeed;
    public float baseAccelMultiplier;
    public float groundDrag;


    private float moveSpeed;
    private bool isSprinting;
    private Vector2 rawMoveInput;
    [SerializeField]private MovementState moveState;
    private InputAction moveInput;
    private InputAction sprintInput;
    private Rigidbody rb;

    [Header("Jumping")]
    public float jumpHeight;
    public float jumpCooldown;
    public float airMultiplier;
    public float gravity;

    private float jumpForce;
    private float apexJumpTime;
    private bool isJumping;
    private bool canJump;
    private InputAction jumpInput;

    [Header("Crouching/Sliding")]
    public float crouchSpeed;
    public float crouchYScale;

    private bool isCrouching;
    private float startYScale;
    private InputAction crouchInput;

    [Header("Ground Check")]
    public float playerHeight;
    public float playerWidth;
    public LayerMask whatIsGround;
    private bool isGrounded;

    [Header("Slopes")]
    public float maxSlopeAngle;

    private bool exitingSlope;
    private RaycastHit slopeHit;

    private void OnValidate()
    {
        // Formula for calculating initial velocity from max height and gravity
        apexJumpTime = Mathf.Sqrt(-2f * jumpHeight / gravity);
        jumpForce = 2f * jumpHeight / apexJumpTime;
    }
    private void OnEnable()
    {
        rightFireInput = InputSystem.actions.FindAction("Right Fire");
        leftFireInput = InputSystem.actions.FindAction("Left Fire");

        moveInput = InputSystem.actions.FindAction("Move");
        sprintInput = InputSystem.actions.FindAction("Sprint");
        crouchInput = InputSystem.actions.FindAction("Crouch");
        jumpInput = InputSystem.actions.FindAction("Jump");


        // Interaction = Press Only
        rightFireInput.performed += RightCharge;
        rightFireInput.canceled += RightFire;
        leftFireInput.performed += LeftCharge;
        leftFireInput.canceled += LeftFire;
        sprintInput.performed += OnSprint;
        sprintInput.canceled += OnSprintCancel;
        crouchInput.performed += OnCrouch;
        crouchInput.canceled += OnCrouchCancel;
        jumpInput.performed += OnJump;
        jumpInput.canceled += OnJumpCancel;


        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;

        ResetJump();
    }
    private void OnDisable()
    {
        rightFireInput.performed -= RightCharge;
        rightFireInput.canceled -= RightFire;
        leftFireInput.performed -= LeftCharge;
        leftFireInput.canceled -= LeftFire;
        sprintInput.performed -= OnSprint;
        sprintInput.canceled -= OnSprintCancel;
        crouchInput.performed -= OnCrouch;
        crouchInput.canceled -= OnCrouchCancel;
        jumpInput.performed -= OnJump;
        jumpInput.canceled -= OnJumpCancel;
    }

    private void FixedUpdate()
    {
        // Ground Check
        // Box cast aligned to player orientation and width. Ideally should be ratio of 0.7 : 1 so box falls within capsule radius
        isGrounded = Physics.BoxCast(transform.position, new Vector3(0.5f, 0f, 0.5f) * playerWidth, Vector3.down, out RaycastHit hit, transform.rotation, playerHeight * 0.5f + 0.05f, whatIsGround);


        Move();
        SpeedControl();
        StateHandler();

        // Apply Drag
        if (isGrounded && canJump)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }
    private void Update()
    {
        // Draw rays from player guns for debugging direciton
        Debug.DrawRay(rightWeapon.transform.position, playerCam.transform.forward * 20f, Color.cyan);
        Debug.DrawRay(leftWeapon.transform.position, playerCam.transform.forward * 20f, Color.cyan);


        // Check for player input changes
        CheckInput();

    }
    private void StateHandler()
    {
        // Priority of states: Crouching > Sprinting > Walking > Airborne

        // Crouching
        if (isGrounded && isCrouching)
        {
            moveState = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        // Sprinting
        else if (isGrounded && isSprinting)
        {
            moveState = MovementState.sprinting;
            moveSpeed = baseSprintSpeed;
        }
        // Walking
        else if (isGrounded)
        {
            moveState = MovementState.walking;
            moveSpeed = baseMoveSpeed;
        }

        // Airborne
        else
        {
            moveState = MovementState.airborne;
        }
    }
    private void CheckInput()
    {
        // Gets the normalized direction vector2 from player input
        rawMoveInput = moveInput.ReadValue<Vector2>();

        if (isJumping && canJump && isGrounded)
        {
            canJump = false;

            Jump();

            // Reset jump after cooldown
            Invoke(nameof(ResetJump), jumpCooldown);
        }


        if (isCrouching)
        {
            // Shrinks player Y scale when crouching, and adds small downward force
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        else
        {
            // Grows player Y scale back to normal
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }
    private void Move()
    {
        Vector3 inputDirection = orientation.forward * rawMoveInput.y + orientation.right * rawMoveInput.x;

        // On Slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(inputDirection) * moveSpeed * baseAccelMultiplier, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // On Ground
        else if (isGrounded)
        {
            rb.AddForce(inputDirection.normalized * moveSpeed * baseAccelMultiplier, ForceMode.Force);
        }

        // In Air
        else if (!isGrounded)
        {
            rb.AddForce(inputDirection.normalized * moveSpeed * baseAccelMultiplier * airMultiplier, ForceMode.Force);

            // Applies gravity to the player while airborne
            rb.AddForce(Vector3.up * gravity, ForceMode.Force);
        }
    }
    private void SpeedControl()
    {
        // Limit speed on a slope differently than on level ground
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }


        // Limits player speed on flat ground
        else
        {
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Prevents player from moving faster than movespeed in x and z directions while ignoring y speed
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
        }

    }
    private void Jump()
    {
        exitingSlope = true;

        // Reset vertical velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Turn off linear drag for the first frame that the grounded check is still active for
        rb.linearDamping = 0f;

        // Apply jumpforce
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }
    private void ResetJump() 
    {
        canJump = true; 
        exitingSlope = false;
    }
    
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.05f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    private Vector3 GetSlopeMoveDirection(Vector3 moveDirection)
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }


    // Input Call Functions
    private void RightCharge(InputAction.CallbackContext context) => rightWeapon.Charge();
    private void LeftCharge(InputAction.CallbackContext context) => leftWeapon.Charge();
    private void RightFire(InputAction.CallbackContext context) => rightWeapon.Shoot(); 
    private void LeftFire(InputAction.CallbackContext context) => leftWeapon.Shoot();
    private void OnSprint(InputAction.CallbackContext context) => isSprinting = true;
    private void OnSprintCancel(InputAction.CallbackContext context) => isSprinting = false;
    private void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = true;
        if (isGrounded)
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }
    private void OnCrouchCancel(InputAction.CallbackContext context) => isCrouching = false;
    private void OnJump(InputAction.CallbackContext context) => isJumping = true;
    private void OnJumpCancel(InputAction.CallbackContext context) => isJumping = false;

    private void OnDrawGizmos()
    {
        if (isGrounded)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position + (Vector3.down * playerHeight * 0.25f), new Vector3(playerWidth, playerHeight * 0.5f + 0.05f, playerWidth));
    }
}
