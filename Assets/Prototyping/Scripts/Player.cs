using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Guns")]
    public Camera playerCam;
    public Weapon rightWeapon;
    public Weapon leftWeapon;
    private InputAction rightFireInput;
    private InputAction leftFireInput;

    [Header("Movement")]
    public float baseMoveSpeed;
    public float baseAccelMultiplier;
    public float groundDrag;
    public float jumpHeight;
    public float jumpCooldown;
    public float airMultiplier;
    public float gravity;
    

    private float jumpForce;
    private float apexJumpTime;
    private bool isJumping;
    private bool canJump;
    private Vector2 rawMoveInput;
    private InputAction moveInput;
    private InputAction jumpInput;
    private Rigidbody rb;

    [Header("Ground Check")]
    public float playerHeight;
    public float playerWidth;
    public LayerMask whatIsGround;
    private bool isGrounded;
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
        jumpInput = InputSystem.actions.FindAction("Jump");

        // Interaction = Press Only
        rightFireInput.performed += RightCharge;
        rightFireInput.canceled += RightFire;
        leftFireInput.performed += LeftCharge;
        leftFireInput.canceled += LeftFire;
        jumpInput.performed += OnJump;
        jumpInput.canceled += OnJumpCancel;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        ResetJump();
    }
    private void OnDisable()
    {
        rightFireInput.performed -= RightCharge;
        rightFireInput.canceled -= RightFire;
        leftFireInput.performed -= LeftCharge;
        leftFireInput.canceled -= LeftFire;
        jumpInput.performed -= OnJump;
        jumpInput.canceled -= OnJumpCancel;
    }
    private void OnDrawGizmos()
    {
        if (isGrounded)
            Gizmos.color = Color.red;
        else 
            Gizmos.color = Color.green;
        
        Gizmos.DrawWireCube(transform.position + (Vector3.down * playerHeight * 0.25f), new Vector3(playerWidth, playerHeight * 0.5f + 0.05f, playerWidth));
    }
    private void FixedUpdate()
    {
        // Ground Check
        // Box cast aligned to player orientation and width. Ideally should be ratio of 0.7 : 1 so box falls within capsule radius
        isGrounded = Physics.BoxCast(transform.position, new Vector3(0.5f, 0f, 0.5f) * playerWidth, Vector3.down, out RaycastHit hit, transform.rotation, playerHeight * 0.5f + 0.05f, whatIsGround);
        

        Move();
        SpeedControl();


        // Apply Drag
        if (isGrounded && canJump)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }
    private void Update()
    {
        // Draw rays from player guns for debugging direciton
        Debug.DrawRay(rightWeapon.transform.position, playerCam.transform.forward*20f, Color.cyan);
        Debug.DrawRay(leftWeapon.transform.position, playerCam.transform.forward*20f, Color.cyan);

        
        // Check for player input changes
        CheckInput();

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
    }
    private void Move()
    {
        Vector3 inputDirection = transform.forward * rawMoveInput.y + transform.right * rawMoveInput.x;

        if (isGrounded)
        {
            rb.AddForce(inputDirection.normalized * baseMoveSpeed * baseAccelMultiplier, ForceMode.Force);
        }

        else if (!isGrounded)
        {
            rb.AddForce(inputDirection.normalized * baseMoveSpeed * baseAccelMultiplier * airMultiplier, ForceMode.Force);

            // Applies gravity to the player while airborne
            rb.AddForce(Vector3.up * gravity, ForceMode.Force);
        }
    }
    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Prevents player from moving faster than movespeed in x and z directions while ignoring y speed
        if (flatVelocity.magnitude > baseMoveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * baseMoveSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }

        
    }
    private void Jump()
    {
        // Reset vertical velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Turn off linear drag for the first frame that the grounded check is still active for
        rb.linearDamping = 0f;

        // Apply jumpforce
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }
    private void ResetJump() => canJump = true;
    private void RightCharge(InputAction.CallbackContext context) => rightWeapon.Charge();
    private void LeftCharge(InputAction.CallbackContext context) => leftWeapon.Charge();
    private void RightFire(InputAction.CallbackContext context) => rightWeapon.Shoot(); 
    private void LeftFire(InputAction.CallbackContext context) => leftWeapon.Shoot();
    private void OnJump(InputAction.CallbackContext context) => isJumping = true;
    private void OnJumpCancel(InputAction.CallbackContext context) => isJumping = false;
}
