using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public Transform reticleParent;

    [Header("HUD Sliders")]
    public Slider rightAmmoSlider;    
    public Slider leftAmmoSlider;
    public Slider rightUtilitySlider;
    public Slider leftUtilitySlider;
    public Slider rightConsumeTimerSlider;
    public Slider LeftConsumeTimerSlider;

    [Header("HUD Ability Icons")]
    public Image RightAbility;
    public Image LeftAbility;
    public Image RightAbilityIcon;
    public Image LeftAbilityIcon;


    [Header("SFX")]
    public string coreCrushSFX;
    public string coreEquipSFX;


    [Header("Pickup Settings")]
    public float pickupRadius;
    public LayerMask whatIsPickupable;
    public TextMeshProUGUI pickupText;

    private List<ManaCorePickup> nearbyCores;
    private ManaCorePickup nearestCore;

    [Header("Weapon Control")]
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
        rightWeapon.currentAmmo = rightWeapon.maxAmmo;
        leftWeapon.currentAmmo = leftWeapon.maxAmmo;
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
        CheckForNearbyCores();

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
        DisplayUIChanges();

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
            if (rightWeapon.Ability.cooldownTimer < rightWeapon.Ability.cooldown)
                rightWeapon.Ability.cooldownTimer += Time.deltaTime;
            else
            {
                GainAbilityCharge(true);
                // If there are still charges to restore, reset the timer
                if (rightAbilityCharges < rightWeapon.displayedMaxCharges)
                    rightWeapon.Ability.cooldownTimer = 0f;
            }
        }
        else
            rightWeapon.Ability.cooldownTimer = 0f;

        // Run the left weapon's cooldown timer
        if (leftAbilityCharges < leftWeapon.displayedMaxCharges)
        {
            if (leftWeapon.Ability.cooldownTimer < leftWeapon.Ability.cooldown)
                leftWeapon.Ability.cooldownTimer += Time.deltaTime;
            else
            {
                GainAbilityCharge(false);
                // If there are still charges to restore, reset the timer
                if (leftAbilityCharges < leftWeapon.displayedMaxCharges)
                    leftWeapon.Ability.cooldownTimer = 0f;
            }
        }
        else
            leftWeapon.Ability.cooldownTimer = 0f;

        // If both weapons are same, countdown buffer timer
        if (hasTwinWeapons)
        {
            if (bufferTimer > 0)
                bufferTimer -= Time.deltaTime;
        }
    }
    private void DisplayUIChanges()
    {
        rightAmmoSlider.value = rightWeapon.currentAmmo;
        rightUtilitySlider.value = rightWeapon.Ability.cooldownTimer / rightWeapon.Ability.cooldown + rightAbilityCharges;
        leftAmmoSlider.value = leftWeapon.currentAmmo;
        leftUtilitySlider.value = leftWeapon.Ability.cooldownTimer / leftWeapon.Ability.cooldown + leftAbilityCharges;
    }
    public void EquipToHand(Weapon weapon, bool toRightHand)
    {
        // Equips a selected weapon to the specified hand
        if (toRightHand)
        {
            // Get and set the weapon stats
            rightWeapon = weapon;
            rWeaponType = weapon.wType;
            rightWeapon.isRightWeapon = true;
            rightAmmoSlider.maxValue = weapon.maxAmmo;
            rightAmmoSlider.value = weapon.currentAmmo;
            rightAmmoSlider.fillRect.GetComponent<Image>().color = weapon.uiColor;
            RightAbility.color = weapon.uiColor;
            weapon.InitiateReticle(reticleParent);
        }
        else
        {
            // Get and set the weapon stats
            leftWeapon = weapon;
            lWeaponType = weapon.wType;
            leftWeapon.isRightWeapon = false;
            leftAmmoSlider.maxValue = weapon.maxAmmo;
            leftAmmoSlider.value = weapon.currentAmmo;
            leftAmmoSlider.fillRect.GetComponent<Image>().color= weapon.uiColor;
            LeftAbility.color = weapon.uiColor;
            leftWeapon.InitiateReticle(reticleParent, 45f);
        }

        // Initializes weapon with important references to player
        weapon.EquipWeapon(playerMovement);

        // Checks if both weapons are the same, with a delay in case this is a weapon swap
        Invoke(nameof(CheckTwinWeapons), 0.01f);
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

        // Have to set both side's abilities again on each weapon switch to ensure neither gets missed when swapping from twin to untwinned or vice versa
        rightUtilitySlider.maxValue = rightWeapon.displayedMaxCharges;
        rightUtilitySlider.value = rightWeapon.Ability.cooldownTimer / rightWeapon.Ability.cooldown + rightAbilityCharges;
        rightUtilitySlider.fillRect.GetComponent<Image>().color = rightWeapon.uiColor;
        leftUtilitySlider.maxValue = leftWeapon.displayedMaxCharges;
        leftUtilitySlider.value = leftWeapon.Ability.cooldownTimer / leftWeapon.Ability.cooldown + leftAbilityCharges;
        leftUtilitySlider.fillRect.GetComponent<Image>().color = leftWeapon.uiColor;
        
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
            // Ensure that there is a core to pickup
            if (nearestCore != null)
            {
                // Check which hand we want to equip it to, but only equip if the hand is empty
                if (isRightWeapon)
                {
                    if (rightWeapon.wType == WeaponType.Empty)
                    {
                        EquipNewWeapon(nearestCore.weaponPrefab, isRightWeapon);
                        nearestCore.PickThisUp();
                        isSwapping = false;
                    }
                }
                else
                {
                    if (leftWeapon.wType == WeaponType.Empty)
                    {
                        EquipNewWeapon(nearestCore.weaponPrefab, isRightWeapon);
                        nearestCore.PickThisUp();
                        isSwapping = false;
                    }
                }
            }
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
        if (isRightWeapon && rightWeapon.wType != WeaponType.Empty)
        {
            rightConsumeTimerSlider.gameObject.SetActive(true);
        } 
        else if (!isRightWeapon && leftWeapon.wType != WeaponType.Empty)
        {
            LeftConsumeTimerSlider.gameObject.SetActive(true);
        }

        float consumeTimer = 0f;
        while (isConsuming && consumeTimer < consumeHoldTime)
        {
            consumeTimer += Time.deltaTime;
            if (isRightWeapon)
            {
                rightConsumeTimerSlider.value = consumeTimer;
            }
            else
            {
                LeftConsumeTimerSlider.value = consumeTimer;
            }
            yield return null;
        }

        // Only completes the coroutine if consume button is still being held at end of timer
        if (isConsuming)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(coreCrushSFX);

            // Destroyes the existing weapon
            if (isRightWeapon)
            {
                Destroy(rightWeapon.reticle.gameObject);
                Destroy(rightWeapon.gameObject);
            }
            else
            {
                Destroy(leftWeapon.reticle.gameObject);
                Destroy(leftWeapon.gameObject);
            }
            // Equips the empty hand in its place
            EquipNewWeapon(emptyHandPrefab, isRightWeapon);
        }

        // Resets the coroutine and allowing it to trigger again
        if (isRightWeapon)
        {
            rightConsumeTimerSlider.gameObject.SetActive(false);
            rightConsumeRoutine = null;
        }
        else 
        {
            LeftConsumeTimerSlider.gameObject.SetActive(false);
            leftConsumeRoutine = null;
        }            
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

        rightWeapon.currentAmmo = tempLeft.currentAmmo;
        leftWeapon.currentAmmo = tempRight.currentAmmo;

        if (tempRight.isFiring && tempLeft.isFiring)
        {
            rightWeapon.PrimaryBegin();
            leftWeapon.PrimaryBegin();
        } 
        else if (tempRight.isFiring)
        {
            rightWeapon.PrimaryRelease();
            leftWeapon.PrimaryBegin();
        }
        else if (tempLeft.isFiring)
        {
            rightWeapon.PrimaryBegin();
            leftWeapon.PrimaryRelease();
        }
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
        {
            hand = rightHand;
            // Removes the empty hand object
            Destroy(rightWeapon.gameObject);
        }

        else
        {
            hand = leftHand;
            // Removes the empty hand object
            Destroy(leftWeapon.gameObject);

        }
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(coreEquipSFX);
        // Creates the new weapon and then equips it to hand
        GameObject newObj = Instantiate(newWeapon, hand);
        Weapon weapon = newObj.GetComponent<Weapon>();
        EquipToHand(weapon, isRightWeapon);
        weapon.currentAmmo = weapon.maxAmmo;
    }
    private void CheckForNearbyCores()
    {
        // Collects an array of all nearby colliders with the pickupable layermask and stores them in a list
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius, whatIsPickupable);
        nearbyCores = new List<ManaCorePickup>();
        foreach (Collider collider in colliders)
        {
            nearbyCores.Add(collider.gameObject.GetComponentInParent<ManaCorePickup>());
        }

        // Checks for which core in range is the closest
        foreach (ManaCorePickup c in nearbyCores)
        {
            if (nearestCore != null)
            {
                if (Vector3.Distance(transform.position, c.transform.position) < Vector3.Distance(transform.position, nearestCore.transform.position))
                    nearestCore = c;
            }
            else
                nearestCore = c;
        }

        // Ensures that if there are no cores in range, we are not storing a core that left range
        if (nearbyCores.Count < 1)
        {
            nearestCore = null;
            pickupText.text = "";
        }
        //Otherwise, display a prompt to pickup the item
        else
        {
            pickupText.text = "Hold 'R' and Right or Left Mouse Click to pickup " + nearestCore.element.ToString() + " weapon.";
        }
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
        if (swapTimer < swapHoldTime && isSwapping && !isConsuming)
            SwapWeapons();
        isSwapping = false;
    }
    private void OnConsumeStart(InputAction.CallbackContext context) => isConsuming = true;
    private void OnConsumeRelease(InputAction.CallbackContext context) => isConsuming = false;

    private void OnDrawGizmos()
    {
        if (nearestCore != null)
            Gizmos.color = Color.aquamarine;
        else
            Gizmos.color = Color.rebeccaPurple;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
