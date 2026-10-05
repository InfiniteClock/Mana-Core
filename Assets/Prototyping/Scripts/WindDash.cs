using UnityEngine;
using UnityEngine.InputSystem;

public class WindDash : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    public bool isRightWeapon;

    private Rigidbody rb;
    private Player player;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;
    public float maxDashYSpeed;
    private Vector3 delayedForceToApply;
    private InputAction dashInput;

    [Header("Cooldown")]
    public float dashCD;

    private float dashCDTimer;

    [Header("Settings")]
    public bool useCameraForward;
    public bool omnidirectionalDash;
    public bool resetVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
    }
    private void OnEnable()
    {
        // Checks if weapon is in right or left hand and applies correct input accordingly
        if (isRightWeapon)
            dashInput = InputSystem.actions.FindAction("Right Utility");
        else
            dashInput = InputSystem.actions.FindAction("Left Utility");

        dashInput.performed += OnDash;
    }
    private void OnDisable()
    {
        dashInput.performed -= OnDash;
    }

    private void Update()
    {
        if (dashCDTimer > 0) dashCDTimer -= Time.deltaTime;
    }
    private void Dash()
    {
        // Returns if dash is still on cooldown
        if (dashCDTimer > 0) return;
        else dashCDTimer = dashCD;

        player.isDashing = true;
        player.maxYSpeed = maxDashYSpeed;

        Transform forwardT;

        if (useCameraForward)
            forwardT = playerCam;
        else 
            forwardT = orientation;

        Vector3 forceToApply = GetDirection(forwardT) * dashForce + orientation.up * dashUpwardForce;

        // Triggers the dash after a very short delay to account for player script order of operations
        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        if (resetVelocity)
        {
            // Cancels all previous movement first
            rb.linearVelocity = Vector3.zero;
        }

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        player.isDashing = false;
        player.maxYSpeed = 0;
    }
    private Vector3 GetDirection(Transform forwardT)
    {
        Vector3 input = new Vector3(player.rawMoveInput.x, 0f, player.rawMoveInput.y);

        Vector3 direction = new Vector3();

        // Dashes forward, or in any input direction, depending on setting
        if (omnidirectionalDash)
            direction = input.z * forwardT.forward + forwardT.right * input.x;
        else
            direction = forwardT.forward;

        if (input.magnitude == 0f)
            direction = forwardT.forward;

        return direction.normalized;
    }


    // Input Call Functions
    private void OnDash(InputAction.CallbackContext context) => Dash();

}
