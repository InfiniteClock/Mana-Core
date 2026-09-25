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
    public float groundDrag;
    //public float baseAccelTime;
    private Vector3 inputDirection;
    private InputAction moveInput;
    private Rigidbody rb;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool isGrounded;
    
    private void OnEnable()
    {
        rightFireInput = InputSystem.actions.FindAction("Right Fire");
        leftFireInput = InputSystem.actions.FindAction("Left Fire");

        moveInput = InputSystem.actions.FindAction("Move");

        // Interaction = Press Only, Press Point = 1
        rightFireInput.performed += RightCharge;
        rightFireInput.canceled += RightFire;
        leftFireInput.performed += LeftCharge;
        leftFireInput.canceled += LeftFire;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    private void OnDisable()
    {
        rightFireInput.performed -= RightCharge;
        rightFireInput.canceled -= RightFire;
        leftFireInput.performed -= LeftCharge;
        leftFireInput.canceled -= LeftFire;
    }

    private void Update()
    {
        // Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        // Draw rays from player guns for debugging direciton
        Debug.DrawRay(rightWeapon.transform.position, playerCam.transform.forward*20f, Color.cyan);
        Debug.DrawRay(leftWeapon.transform.position, playerCam.transform.forward*20f, Color.cyan);


        // Player motion
        Move();
        SpeedControl();

        // Apply Drag
        if (isGrounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        //Debug.Log("Player speed = " + rb.linearVelocity.magnitude);
    }
    private void Move()
    {
        // Gets the normalized direction vector2 from player input
        Vector2 movement = moveInput.ReadValue<Vector2>();

        inputDirection = playerCam.transform.forward * movement.y + playerCam.transform.right * movement.x;

        rb.AddForce(inputDirection.normalized * baseMoveSpeed, ForceMode.Force);
        
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
    private void RightCharge(InputAction.CallbackContext context) => rightWeapon.Charge();
    private void LeftCharge(InputAction.CallbackContext context) => leftWeapon.Charge();
    private void RightFire(InputAction.CallbackContext context) => rightWeapon.Shoot(); 
    private void LeftFire(InputAction.CallbackContext context) => leftWeapon.Shoot(); 
    
}
