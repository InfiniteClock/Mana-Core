using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapons : MonoBehaviour
{
    [Header("References")]
    public Camera playerCam;
    public Weapon rightWeapon;
    public Weapon leftWeapon;

    private PlayerMovement playerMovement;
    private Rigidbody rb;
    private Vector3 camTarget;
    private RaycastHit hit;
    private InputAction rightFireInput;
    private InputAction leftFireInput;
    private InputAction rightUtilityInput;
    private InputAction leftUtilityInput;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        EquipToHand(rightWeapon, true);
        EquipToHand(leftWeapon, false);
    }

    private void OnEnable()
    {
        rightFireInput = InputSystem.actions.FindAction("Right Fire");
        leftFireInput = InputSystem.actions.FindAction("Left Fire");
        rightUtilityInput = InputSystem.actions.FindAction("Right Utility");
        leftUtilityInput = InputSystem.actions.FindAction("Left Utility");

        rightFireInput.performed += OnRightPrimaryStart;
        rightFireInput.canceled += OnRightPrimaryRelease;
        rightUtilityInput.performed += OnRightSecondaryStart;
        rightUtilityInput.canceled += OnRightSecondaryRelease;
        leftFireInput.performed += OnLeftPrimaryStart;
        leftFireInput.canceled += OnLeftPrimaryRelease;
        leftUtilityInput.performed += OnLeftSecondaryStart;
        leftUtilityInput.canceled += OnLeftSecondaryRelease;
    }
    private void OnDisable()
    {
        rightFireInput.performed -= OnRightPrimaryStart;
        rightFireInput.canceled -= OnRightPrimaryRelease;
        rightUtilityInput.performed -= OnRightSecondaryStart;
        rightUtilityInput.canceled -= OnRightSecondaryRelease;
        leftFireInput.performed -= OnLeftPrimaryStart;
        leftFireInput.canceled -= OnLeftPrimaryRelease;
        leftUtilityInput.performed -= OnLeftSecondaryStart;
        leftUtilityInput.canceled -= OnLeftSecondaryRelease;

    }
    private void FixedUpdate()
    {
        // Gets the velocity of the player to add to the gun's firing
        rightWeapon.playerVelocity = rb.linearVelocity;
        leftWeapon.playerVelocity = rb.linearVelocity;

        // Determines target based on what camera is looking at
        Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit);

        if (hit.point == Vector3.zero || hit.distance > 200f)
            hit.point = playerCam.transform.forward * 200f;

        camTarget = hit.point;
        rightWeapon.target = hit.point;
        leftWeapon.target = hit.point;
    }
    private void Update()
    {
        // Draw rays from player guns for debugging direction
        Debug.DrawRay(rightWeapon.projectileSpawnPoint.position, (camTarget - rightWeapon.projectileSpawnPoint.position).normalized * 20f, Color.cyan);
        Debug.DrawRay(leftWeapon.projectileSpawnPoint.position, (camTarget - leftWeapon.projectileSpawnPoint.position).normalized * 20f, Color.cyan);

        Debug.DrawRay(playerCam.transform.position, playerCam.transform.forward * 100f, Color.coral);
    }
    public void EquipToHand(Weapon weapon, bool toRightHand)
    {
        // Equips a selected weapon to the specified hand
        if (toRightHand)
            rightWeapon = weapon;
        else
            leftWeapon = weapon;

        // Initializes weapon with important references to player
        weapon.EquipWeapon(playerMovement);
    }
    private void OnRightPrimaryStart(InputAction.CallbackContext context) => rightWeapon.PrimaryBegin();
    private void OnRightPrimaryRelease(InputAction.CallbackContext context) => rightWeapon.PrimaryRelease();
    private void OnRightSecondaryStart(InputAction.CallbackContext context) => rightWeapon.SecondaryBegin();
    private void OnRightSecondaryRelease(InputAction.CallbackContext context) => rightWeapon.SecondaryRelease();
    private void OnLeftPrimaryStart(InputAction.CallbackContext context) => leftWeapon.PrimaryBegin();
    private void OnLeftPrimaryRelease(InputAction.CallbackContext context) => leftWeapon.PrimaryRelease();
    private void OnLeftSecondaryStart(InputAction.CallbackContext context) => leftWeapon.SecondaryBegin();
    private void OnLeftSecondaryRelease(InputAction.CallbackContext context) => leftWeapon.SecondaryRelease();
}
