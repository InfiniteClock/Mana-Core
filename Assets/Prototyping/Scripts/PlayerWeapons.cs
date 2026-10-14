using System.Collections;
using TMPro;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapons : MonoBehaviour
{
    [Header("References")]
    public Camera playerCam;
    public Transform rightHand;
    public Transform leftHand;
    public Weapon rightWeapon;
    public Weapon leftWeapon;
    public TextMeshProUGUI rightAbilityText;
    public TextMeshProUGUI leftAbilityText;
    public GameObject emptyHandPrefab;
    public float dashBuffer;
    public float swapHoldTime;
    public float consumeHoldTime;

    public bool hasTwinWeapons { get; private set; }

    //[HideInInspector]
    public int rightAbilityCharges;
    //[HideInInspector]
    public int leftAbilityCharges;
    //[HideInInspector]
    public float bufferTimer;
    //[HideInInspector]
    public float swapTimer;

    private bool isSwapping;
    private bool isConsuming;
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
    private InputAction swapInput;
    private InputAction consumeInput;
    private Coroutine rightConsumeRoutine;
    private Coroutine leftConsumeRoutine;

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
        swapInput = InputSystem.actions.FindAction("Swap");
        consumeInput = InputSystem.actions.FindAction("Consume");


        rightFireInput.performed += OnRightPrimaryStart;
        rightFireInput.canceled += OnRightPrimaryRelease;
        rightUtilityInput.performed += OnRightSecondaryStart;
        rightUtilityInput.canceled += OnRightSecondaryRelease;
        leftFireInput.performed += OnLeftPrimaryStart;
        leftFireInput.canceled += OnLeftPrimaryRelease;
        leftUtilityInput.performed += OnLeftSecondaryStart;
        leftUtilityInput.canceled += OnLeftSecondaryRelease;
        swapInput.performed += OnSwapStart;
        swapInput.canceled += OnSwapRelease;
        consumeInput.performed += OnConsumeStart;
        consumeInput.canceled += OnConsumeRelease;
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
        swapInput.performed -= OnSwapStart;
        swapInput.canceled -= OnSwapRelease;
        consumeInput.performed -= OnConsumeStart;
        consumeInput.canceled -= OnConsumeRelease;
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

        if (isSwapping)
        {
            if (swapTimer < swapHoldTime)
                swapTimer += Time.deltaTime;
        }
        else
            swapTimer = 0f;
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

        // Checks if both weapons are the same, with a delay in case this is a weapon swap
        Invoke(nameof(CheckTwinWeapons), 0.01f);

        // Initializes weapon with important references to player
        weapon.EquipWeapon(playerMovement);
    }
    private void CheckTwinWeapons()
    {
        hasTwinWeapons = rightWeapon.wType == leftWeapon.wType;

        // Set the ability charges based on if both weapons are the same, then based on whether it is left or right weapon
        if (hasTwinWeapons)
        {
            rightWeapon.displayedMaxCharges = rightWeapon.maxAbilityCharges + leftWeapon.maxAbilityCharges;
            leftWeapon.displayedMaxCharges = rightWeapon.displayedMaxCharges;
            // Ensures we cannot surpass the min of 0 and maximum amount of charges across both weapons
            rightAbilityCharges = Mathf.Clamp(rightAbilityCharges + leftAbilityCharges, 0, rightWeapon.displayedMaxCharges);
            leftAbilityCharges = rightAbilityCharges;
        }
        else
        {
            rightWeapon.displayedMaxCharges = rightWeapon.maxAbilityCharges;
            leftWeapon.displayedMaxCharges = leftWeapon.maxAbilityCharges;
            // Ensures we never have more ability charges than able when we swap off of twin weapons
            rightAbilityCharges = Mathf.Min(rightAbilityCharges, rightWeapon.maxAbilityCharges);
            leftAbilityCharges = Mathf.Min(leftAbilityCharges, leftWeapon.maxAbilityCharges);
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

    private void CheckPrimaryStart(bool isRightWeapon)
    {
        // Check if player is holding consume input first
        if (isConsuming)
        {
            // Only allows the consume coroutine to start if its not already running for that hand
            if (isRightWeapon)
                rightConsumeRoutine ??= StartCoroutine(ConsumeWeapon(isRightWeapon));
            else
                leftConsumeRoutine ??= StartCoroutine(ConsumeWeapon(isRightWeapon));
        }

        // Check if player is holding swap input next
        else if (isSwapping)
        {
            Debug.Log("Here is where I'd swap to a new Mana Core... If I had one!" + isRightWeapon);
        }

        // Regular shooting otherwise
        else
        {
            if (isRightWeapon)
                rightWeapon.PrimaryBegin();
            else
                leftWeapon.PrimaryBegin();
        }
    }
    private IEnumerator ConsumeWeapon(bool isRightWeapon)
    {
        float consumeTimer = 0f;
        while (isConsuming && consumeTimer < consumeHoldTime)
        {
            consumeTimer += Time.deltaTime;
            yield return null;
        }

        // Only completes the coroutine if consume button is still being held at end of timer
        if (isConsuming)
        {
            // Destroyes the existing weapon
            if (isRightWeapon)
                Destroy(rightWeapon.gameObject);
            else
                Destroy(leftWeapon.gameObject);
            // Equips the empty hand in its place
            EquipNewWeapon(emptyHandPrefab, isRightWeapon);
        }

        // Resets the coroutine and allowing it to trigger again
        if (isRightWeapon)
            rightConsumeRoutine = null;
        else
            leftConsumeRoutine = null;
    }
    private void SwapWeapons()
    {
        Weapon tempRight = rightWeapon;
        Weapon tempLeft = leftWeapon;

        rightWeapon.transform.SetParent(leftHand);
        leftWeapon.transform.SetParent(rightHand);

        rightWeapon.transform.localPosition = Vector3.zero;
        leftWeapon.transform.localPosition = Vector3.zero;

        int tempRightCharges = rightAbilityCharges;
        int tempLeftCharges = leftAbilityCharges;
        rightAbilityCharges = tempLeftCharges;
        leftAbilityCharges = tempRightCharges;

        EquipToHand(tempRight, false);
        EquipToHand(tempLeft, true);
    }
    /// <summary>
    /// Instantiates the new weapon and sets its transform position, then equips it
    /// </summary>
    /// <param name="newWeapon">The prefab of the weapon to spawn</param>
    /// <param name="isRightWeapon">Is this going into right hand or left</param>
    private void EquipNewWeapon(GameObject newWeapon, bool isRightWeapon)
    {
        Transform hand;
        if (isRightWeapon)
            hand = rightHand;

        else
            hand = leftHand;
        GameObject newObj = Instantiate(newWeapon, hand);
        EquipToHand(newObj.GetComponent<Weapon>(), isRightWeapon);
    }


    private void OnRightPrimaryStart(InputAction.CallbackContext context) => CheckPrimaryStart(true);
    private void OnRightPrimaryRelease(InputAction.CallbackContext context) => rightWeapon.PrimaryRelease();
    private void OnRightSecondaryStart(InputAction.CallbackContext context) => rightWeapon.SecondaryBegin();
    private void OnRightSecondaryRelease(InputAction.CallbackContext context) => rightWeapon.SecondaryRelease();
    private void OnLeftPrimaryStart(InputAction.CallbackContext context) => CheckPrimaryStart(false);
    private void OnLeftPrimaryRelease(InputAction.CallbackContext context) => leftWeapon.PrimaryRelease();
    private void OnLeftSecondaryStart(InputAction.CallbackContext context) => leftWeapon.SecondaryBegin();
    private void OnLeftSecondaryRelease(InputAction.CallbackContext context) => leftWeapon.SecondaryRelease();
    private void OnSwapStart(InputAction.CallbackContext context) => isSwapping = true;
    private void OnSwapRelease(InputAction.CallbackContext context) 
    {
        if (swapTimer < swapHoldTime)
            SwapWeapons();
        isSwapping = false;
    }
    private void OnConsumeStart(InputAction.CallbackContext context) => isConsuming = true;
    private void OnConsumeRelease(InputAction.CallbackContext context) => isConsuming = false;
}
