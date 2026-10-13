using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapons : MonoBehaviour
{
    [Header("References")]
    public Camera playerCam;
    public Weapon rightWeapon;
    public Weapon leftWeapon;
    public TextMeshProUGUI rightAbilityText;
    public TextMeshProUGUI leftAbilityText;
    public float dashBuffer;

    public bool hasTwinWeapons { get; private set; }

    //[HideInInspector]
    public int rightAbilityCharges;
    //[HideInInspector]
    public int leftAbilityCharges;
    //[HideInInspector]
    public float bufferTimer;

    private PlayerMovement playerMovement;
    private Rigidbody rb;
    private WeaponType rWeaponType;
    private WeaponType lWeaponType;
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
        ControlTimers();
        DisplayAbilityCooldowns();

        // Draw rays from player guns for debugging direction
        Debug.DrawRay(rightWeapon.projectileSpawnPoint.position, (camTarget - rightWeapon.projectileSpawnPoint.position).normalized * 20f, Color.cyan);
        Debug.DrawRay(leftWeapon.projectileSpawnPoint.position, (camTarget - leftWeapon.projectileSpawnPoint.position).normalized * 20f, Color.cyan);

        Debug.DrawRay(playerCam.transform.position, playerCam.transform.forward * 100f, Color.coral);
    }
    private void ControlTimers()
    {
        // Run the right weapon's cooldown timer
        if (rightAbilityCharges < rightWeapon.displayedMaxCharges)
        {
            if (rightWeapon.Ability.cooldownTimer > 0)
                rightWeapon.Ability.cooldownTimer -= Time.deltaTime;
            else
            {
                GainAbilityCharge(true);
                // If there are still charges to restore, reset the timer
                if (rightAbilityCharges < rightWeapon.displayedMaxCharges)
                    rightWeapon.Ability.cooldownTimer = rightWeapon.Ability.cooldown;
            }
        }
        else
            rightWeapon.Ability.cooldownTimer = rightWeapon.Ability.cooldown;

        // Run the left weapon's cooldown timer
        if (leftAbilityCharges < leftWeapon.displayedMaxCharges)
        {
            if (leftWeapon.Ability.cooldownTimer > 0)
                leftWeapon.Ability.cooldownTimer -= Time.deltaTime;
            else
            {
                GainAbilityCharge(false);
                // If there are still charges to restore, reset the timer
                if (leftAbilityCharges < leftWeapon.displayedMaxCharges)
                    leftWeapon.Ability.cooldownTimer = leftWeapon.Ability.cooldown;
            }
        }
        else
            leftWeapon.Ability.cooldownTimer = leftWeapon.Ability.cooldown;

        // If both weapons are same, countdown buffer timer
        if (hasTwinWeapons)
        {
            if (bufferTimer > 0)
                bufferTimer -= Time.deltaTime;
        }
    }
    private void DisplayAbilityCooldowns()
    {
        rightAbilityText.text = "Ability: " + rightWeapon.wType + " : " + rightAbilityCharges.ToString("0") + "/" + rightWeapon.displayedMaxCharges.ToString("0")
            + "\nAmmo : " + rightWeapon.currentAmmo+"/"+rightWeapon.maxAmmo;

        leftAbilityText.text = "Ability: " + leftWeapon.wType + " : " + leftAbilityCharges.ToString("0") + "/" + leftWeapon.displayedMaxCharges.ToString("0")
            + "\nAmmo : " + leftWeapon.currentAmmo.ToString("0") + "/" + leftWeapon.maxAmmo.ToString("0");
    }
    public void EquipToHand(Weapon weapon, bool toRightHand)
    {
        // Equips a selected weapon to the specified hand
        if (toRightHand)
        {
            rightWeapon = weapon;
            rWeaponType = weapon.wType;
            rightWeapon.isRightWeapon = true;
        }
        else
        {
            leftWeapon = weapon;
            lWeaponType = weapon.wType;
            leftWeapon.isRightWeapon = false;
        }

        // Checks if both weapons are the same
        hasTwinWeapons = rightWeapon.wType == leftWeapon.wType;

        // Initializes weapon with important references to player
        weapon.EquipWeapon(playerMovement);

        // Set the ability charges based on if both weapons are the same, then based on whether it is left or right weapon
        if (hasTwinWeapons)
        {
            rightWeapon.displayedMaxCharges = rightWeapon.maxAbilityCharges + leftWeapon.maxAbilityCharges;
            leftWeapon.displayedMaxCharges = rightWeapon.displayedMaxCharges;
            rightAbilityCharges = 0;
            leftAbilityCharges = 0;
        }
        else if (toRightHand)
        {
            rightWeapon.displayedMaxCharges = rightWeapon.maxAbilityCharges;
            rightAbilityCharges = 0;
        }
        else
        {
            leftWeapon.displayedMaxCharges = leftWeapon.maxAbilityCharges;
            leftAbilityCharges = 0;
        }
    }
    public void GainAbilityCharge(bool usedRightWeapon)
    {
        if (hasTwinWeapons)
        {
            rightAbilityCharges++;
            leftAbilityCharges++;
        }
        else if (usedRightWeapon)
            rightAbilityCharges++;
        else
            leftAbilityCharges++;
    }
    public void UseAbilityCharge(bool usedRightWeapon)
    {
        if (hasTwinWeapons)
        {
            rightAbilityCharges--;
            leftAbilityCharges--;
        }
        else if (usedRightWeapon)
            rightAbilityCharges--;
        else
            leftAbilityCharges--;
    }
    /// <summary>
    /// Returns the amount of remaining ability charges for that hand's ability
    /// </summary>
    /// <param name="usedRightWeapon">Is this the right weapon?</param>
    /// <returns></returns>
    public int GetAbilityCharges(bool usedRightWeapon)
    {
        return usedRightWeapon ? rightAbilityCharges : leftAbilityCharges;
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
