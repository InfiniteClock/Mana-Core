using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum MovementState { walking, sprinting, crouching, sliding, airborne }

    [Header("Guns")]
    public Camera playerCam;
    public Weapon rightWeapon;
    public Weapon leftWeapon;
    public TextMeshProUGUI speedText;

    private InputAction rightFireInput;
    private InputAction leftFireInput;

    [Header("Movement")]
    public Transform orientation;
    public float baseMoveSpeed;
    public float baseSprintSpeed;
    public float baseSlideSpeed;
    public float speedIncreaseMulti;
    public float slopeIncreaseMulti;
    public float baseAccelMultiplier;
    public float groundDrag;

    
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private bool isSprinting;
    private Vector2 rawMoveInput;
    private Vector3 inputDirection;
    private MovementState moveState;
    private InputAction moveInput;
    private InputAction sprintInput;
    private Rigidbody rb;
    private Coroutine momentumRoutine;

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
    public float maxSlideTime;
    public float slideForce;
    public float minSpeedToSlide;

    private bool isCrouching;
    private bool isSliding;
    private bool canStand;
    private float slideTimer;
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
        apexJumpTime = Mathf.Sqrt(-2f * jumpHeight / -gravity);
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
        Move();
        StateHandler();
    }
    private void Update()
    {
        // Ground Check
        // Box cast aligned to player orientation and width. Ideally should be ratio of 0.7 : 1 so box falls within capsule radius
        isGrounded = Physics.BoxCast(transform.position, new Vector3(0.5f, 0f, 0.5f) * playerWidth, Vector3.down, out RaycastHit hit, transform.rotation, playerHeight * 0.5f + 0.05f, whatIsGround);

        // Ceiling Check
        // Box cast aligned to player orientation and width. Same as ground check but for above instead of below;
        canStand = !Physics.BoxCast(transform.position, new Vector3(0.5f, 0f, 0.5f) * playerWidth, Vector3.up, out RaycastHit upHit, transform.rotation, playerHeight * 0.5f + 0.05f, whatIsGround);

        // Draw rays from player guns for debugging direciton
        Debug.DrawRay(rightWeapon.transform.position, playerCam.transform.forward * 20f, Color.cyan);
        Debug.DrawRay(leftWeapon.transform.position, playerCam.transform.forward * 20f, Color.cyan);


        // Check for player input changes
        CheckInput();
        SpeedControl();


        // Apply Drag
        if (isGrounded && canJump)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        // Debugging Text
        Vector3 hSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        speedText.text = ("True Speed : "+rb.linearVelocity.magnitude+"\nHorizontal Speed : " + hSpeed.magnitude + "\nVertical Speed : " + rb.linearVelocity.y + "\nSlope Angle : " + Vector3.Angle(Vector3.up, slopeHit.normal));
    }
    private void StateHandler()
    {
        // Priority of states: Sliding > Crouching > Sprinting > Walking > Airborne

        // Sliding
        if (isSliding)
        {
            moveState = MovementState.sliding;

            // Apply sliding speed if moving downhill or on flat
            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = baseSlideSpeed;
            // Apply sprint speed if moving uphill
            else
                desiredMoveSpeed = baseSprintSpeed;
        }

        // Crouching
        else if (isGrounded && CrouchBuffer())
        {
            moveState = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // Sprinting
        else if (isGrounded && isSprinting)
        {
            moveState = MovementState.sprinting;
            desiredMoveSpeed = baseSprintSpeed;
        }
        // Walking
        else if (isGrounded)
        {
            moveState = MovementState.walking;
            desiredMoveSpeed = baseMoveSpeed;
        }

        // Airborne
        else
        {
            moveState = MovementState.airborne;
        }

        // Check if desired move speed has changed drastically (and current move speed isn't 0) - if so, run the Lerp coroutine
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            //Debug.Log("Drastic Speed Inrease Detected!");
            if (momentumRoutine != null) 
                StopCoroutine(momentumRoutine);
            momentumRoutine = StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;

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


        if (CrouchBuffer())
        {
            // Sets sliding to true if player is moving fast enough AND is applying movement input AND the slide timer hasn't ended
            if (rb.linearVelocity.magnitude > minSpeedToSlide && rawMoveInput.magnitude > 0.5f && slideTimer > 0f)
                isSliding = true;
            else
                isSliding = false;

            // Shrinks player Y scale when crouching, and adds small downward force
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        else
        {
            // Grows player Y scale back to normal
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            isSliding = false;
        }
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // Lerps movement speed to desired value
        float time = 0f;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1f + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMulti * slopeIncreaseMulti * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMulti;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }
    private void Move()
    {
        inputDirection = orientation.forward * rawMoveInput.y + orientation.right * rawMoveInput.x;

        // Sliding
        if (isSliding)
        {
            if (!OnSlope() || rb.linearVelocity.y > -0.1f)
            {
                rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

                // Counts timer down only if sliding on level terrain or uphill
                slideTimer -= Time.deltaTime;

                if (isGrounded)
                    rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            else
            {
                rb.AddForce(GetSlopeMoveDirection() * slideForce, ForceMode.Force);
            }
        }

        // On Slope
        else if (OnSlope() && !exitingSlope)
        {
            // Requires 2x acceleration multiplier on slopes due to higher downward force and friction
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * baseAccelMultiplier * 2f, ForceMode.Force);

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
        }

        // Applies gravity to the player while airborne ALWAYS
        if (!isGrounded)
            rb.AddForce(Vector3.down * gravity, ForceMode.Force);
    }
    private void SpeedControl()
    {
        // Limit speed on a slope differently than on level ground
        if (OnSlope() && !exitingSlope && isGrounded)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }


        // Limits player speed on flat ground and in air
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
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(inputDirection, slopeHit.normal).normalized;
    }
    private void CrouchStart()
    {
        isCrouching = true;
        slideTimer = maxSlideTime;
        if (isGrounded)
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }
    private bool CrouchBuffer()
    {
        if (!isCrouching)
        {
            if (!canStand)
                return true;
            else
                return false;
        }
        else
        {
            return true;
        }
    }

    // Input Call Functions
    private void RightCharge(InputAction.CallbackContext context) => rightWeapon.Charge();
    private void LeftCharge(InputAction.CallbackContext context) => leftWeapon.Charge();
    private void RightFire(InputAction.CallbackContext context) => rightWeapon.Shoot(); 
    private void LeftFire(InputAction.CallbackContext context) => leftWeapon.Shoot();
    private void OnSprint(InputAction.CallbackContext context) => isSprinting = true;
    private void OnSprintCancel(InputAction.CallbackContext context) => isSprinting = false;
    private void OnCrouch(InputAction.CallbackContext context) => CrouchStart();
    private void OnCrouchCancel(InputAction.CallbackContext context) => isCrouching = false;
    private void OnJump(InputAction.CallbackContext context) => isJumping = true;
    private void OnJumpCancel(InputAction.CallbackContext context) => isJumping = false;

    private void OnDrawGizmos()
    {
        if (isGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawWireCube(transform.position + (Vector3.down * playerHeight * 0.25f), new Vector3(playerWidth, playerHeight * 0.5f + 0.05f, playerWidth));

        if (canStand)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.blue;

        Gizmos.DrawWireCube(transform.position + (Vector3.up * playerHeight * 0.25f), new Vector3(playerWidth, playerHeight * 0.5f + 0.05f, playerWidth));
    }
}
