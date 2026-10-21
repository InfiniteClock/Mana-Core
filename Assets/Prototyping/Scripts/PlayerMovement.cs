using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : MonoBehaviour
{
    public enum MovementState { walking, crouching, sliding, airborne, dashing }

    [Header("VFX Controller")]
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private string dashingFeatureName = "Dash VFX";
    private ScriptableRendererFeature dashVFX;

    [Header("Camera")]
    public Camera playerCam;
    public TextMeshProUGUI speedText;

    [Header("Movement")]
    public Transform orientation;
    public float baseMoveSpeed;
    public float baseSlideSpeed;
    public float baseSpeedChangeFactor;
    public float slopeIncreaseMulti;
    public float baseAccelMultiplier;
    public float groundDrag;
    public string jumpSound;

    public MovementState moveState { get; private set; }
    private MovementState lastMoveState;
    public float moveSpeed { get; private set; }
    public float desiredMoveSpeed { get; private set; }
    public float lastDesiredMoveSpeed { get; private set; }
    
    private float speedChangeFactor;
    private bool keepMomentum;
    public Vector2 rawMoveInput { get; private set; }
    private Vector3 inputDirection;
    
    private InputAction moveInput;
    private Rigidbody rb;
    private Coroutine momentumRoutine;

    [Header("Jumping")]
    public float jumpHeight;
    public float jumpCooldown;
    public float airMultiplier;
    public float gravity;

    public float jumpForce;
    public float apexJumpTime;
    private bool isJumping;
    private bool canJump;
    private InputAction jumpInput;

    [Header("Crouching/Sliding")]
    public float baseCrouchSpeed;
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

    [Header("Abilities")]
    public float baseDashSpeed;
    public float dashSpeedChangeFactor;

    // Following are public so other scripts can access, but not set in inspector
    [HideInInspector]
    public bool isDashing;
    [HideInInspector]
    public float maxYSpeed;
    [HideInInspector]
    public bool isCloaked;
    [HideInInspector]
    public float cloakSpeedMulti;
    [HideInInspector]
    public float cloakGravMod;
    [HideInInspector]
    public float cloakAirMod;
    // --- -- -
    
    [Header("Ground Check")]
    public float playerHeight;
    public float playerWidth;
    public LayerMask whatIsGround;

    public bool isGrounded { get; private set; }

    [Header("Slopes")]
    public float maxSlopeAngle;

    private bool exitingSlope;
    private RaycastHit slopeHit;

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetJumpStats(jumpHeight);
        cloakSpeedMulti = 1f;
        cloakGravMod = 1f;
        cloakAirMod = 1f;
        speedChangeFactor = baseSpeedChangeFactor;

        if (rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature.name == dashingFeatureName)
                {
                    dashVFX = feature;
                    break;
                }
            }
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;

        ResetJump();
    }

#else
    private void Start()
    {
        SetJumpStats(jumpHeight);
        cloakSpeedMulti = 1f;
        cloakGravMod = 1f;
        cloakAirMod = 1f;
        speedChangeFactor = baseSpeedChangeFactor;

        if (rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature.name == dashingFeatureName)
                {
                    dashVFX = feature;
                    break;
                }
            }
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;

        ResetJump();
    }
#endif
    private void OnEnable()
    {
        moveInput = InputSystem.actions.FindAction("Move");
        crouchInput = InputSystem.actions.FindAction("Crouch");
        jumpInput = InputSystem.actions.FindAction("Jump");


        // Interaction = Press Only
        crouchInput.performed += OnCrouch;
        crouchInput.canceled += OnCrouchCancel;
        jumpInput.performed += OnJump;
        jumpInput.canceled += OnJumpCancel;
    }
    private void OnDisable()
    {
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

        // Ceiling Check
        // Box cast aligned to player orientation and width. Same as ground check but for above instead of below;
        canStand = !Physics.BoxCast(transform.position, new Vector3(0.5f, 0f, 0.5f) * playerWidth, Vector3.up, out RaycastHit upHit, transform.rotation, playerHeight * 0.5f + 0.05f, whatIsGround);

        // Apply Drag when in a grounded state only (except in initial stages of jumping)
        if ( canJump && 
            (moveState == MovementState.walking ||
            moveState == MovementState.crouching ||
            moveState == MovementState.sliding))
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        Move();
        
    }
    private void Update()
    {
        // Check for player input changes
        CheckInput();
        StateHandler();
        SpeedControl();

        // Debugging Text
        Vector3 hSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        speedText.text = ("True Speed : "+rb.linearVelocity.magnitude.ToString("0.0") +
            "\nDesired Speed : " + desiredMoveSpeed +
            "\nHorizontal Speed : " + hSpeed.magnitude.ToString("0.0") + 
            "\nVertical Speed : " + rb.linearVelocity.y.ToString("0.0") + 
            "\nCurrent State : " + moveState.ToString() +
            "\nSlope Angle : " + Vector3.Angle(Vector3.up, slopeHit.normal));
    }
    private void StateHandler()
    {
        // Priority of states: Dashing > Sliding > Crouching > Sprinting > Walking > Airborne

        if (isDashing)
        {
            moveState = MovementState.dashing;
            desiredMoveSpeed = baseDashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        // Sliding
        else if (isGrounded && isSliding)
        {
            moveState = MovementState.sliding;

            // Initiate with a boost of speed into the dash
            if (lastMoveState != MovementState.sliding)
                moveSpeed = baseMoveSpeed * cloakSpeedMulti;

            // Apply sliding speed if moving downhill
            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = baseSlideSpeed * cloakSpeedMulti;
            }
            // Apply crouch speed if moving uphill or on flat
            else
            {
                desiredMoveSpeed = baseMoveSpeed * cloakSpeedMulti;
            }
        }

        // Crouching
        else if (isGrounded && CrouchBuffer())
        {
            moveState = MovementState.crouching;
            desiredMoveSpeed = baseCrouchSpeed * cloakSpeedMulti;
        }
        // Walking
        else if (isGrounded)
        {
            moveState = MovementState.walking;
            desiredMoveSpeed = baseMoveSpeed * cloakSpeedMulti;
        }

        // Airborne
        else
        {
            moveState = MovementState.airborne;
            desiredMoveSpeed = baseMoveSpeed * cloakSpeedMulti;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        // If the last state was dashing, sliding, or crouching after sliding, use momentum lerp
        if (lastMoveState == MovementState.dashing || moveState == MovementState.sliding || 
            (lastMoveState == MovementState.sliding && moveState == MovementState.crouching)) keepMomentum = true;

        // Otherwise, if the current state is walking or crouching, do NOT use momentum lerp
        else if (moveState == MovementState.walking || moveState == MovementState.crouching) keepMomentum = false;


        if (desiredMoveSpeedHasChanged)
        {
            // Stop the coroutine when desired move speed changes - we are done lerping regardless
            if (momentumRoutine != null)
                StopCoroutine(momentumRoutine);

            if (keepMomentum)
            {
                momentumRoutine = StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        if (moveState == MovementState.dashing)
            dashVFX.SetActive(true);
        else
            dashVFX.SetActive(false);


        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastMoveState = moveState;
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

                time += Time.deltaTime * speedChangeFactor * slopeIncreaseMulti * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedChangeFactor;
            yield return null;
        }

        speedChangeFactor = baseSpeedChangeFactor;
        moveSpeed = desiredMoveSpeed;
        keepMomentum = false;
    }
    private void Move()
    {
        // Ignore regular movement control if dashing
        if (moveState == MovementState.dashing) return;

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
            rb.AddForce(inputDirection.normalized * moveSpeed * baseAccelMultiplier * airMultiplier * cloakAirMod, ForceMode.Force);
        }

        // Applies gravity to the player while airborne and not dashing
        if (!isGrounded)
            rb.AddForce(Vector3.down * gravity * cloakGravMod, ForceMode.Force);
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

        // Limit Y velocity
        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
    }
    private void Jump()
    {
        // Don't jump if the ceiling is too low
        if (!canStand) return;

        exitingSlope = true;

        AudioManager.Instance.PlayOneShot(jumpSound, gameObject);

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
    
    public void SetJumpStats(float height)
    {
        // Formula for calculating initial velocity from max height and gravity
        apexJumpTime = Mathf.Sqrt(-2f * height / -gravity);
        jumpForce = 2f * height / apexJumpTime;
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
    private void OnCrouch(InputAction.CallbackContext context) => CrouchStart();
    private void OnCrouchCancel(InputAction.CallbackContext context) => isCrouching = false;
    private void OnJump(InputAction.CallbackContext context) => isJumping = true;
    private void OnJumpCancel(InputAction.CallbackContext context) => isJumping = false;

    // Draw specialized Gizmos in Editor
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
