using UnityEngine;
using System.Collections.Generic;

public enum WeaponType { Shadow, Wind, Fire, Ice, Earth, Lightning, Empty }
public abstract class Weapon : MonoBehaviour
{
    protected PlayerMovement playerMovement;
    protected Rigidbody rb;
    public Ability Ability { get; protected set; }

    [Header("Shooting")]
    public GameObject projectile;
    public WeaponType wType;

    [HideInInspector]
    public bool isRightWeapon;
    [HideInInspector]
    public Vector3 target;
    [HideInInspector]
    public Vector3 playerVelocity;
    public bool isFiring { get; protected set; }

    [Header("Energy")]
    public float maxAmmo;
    public float ammoPerShot;

    [HideInInspector]
    public float currentAmmo;

    [Header("Input & Display")]
    public Transform projectileSpawnPoint;
    public Color uiColor;
    public GameObject reticlePrefab;

    [HideInInspector]
    public Reticle reticle;

    [Header("Bloom")]
    [Tooltip("Base spread in degrees")]
    public float baseSpread;
    [Tooltip("How much bloom is added per shot in degrees")]
    public float bloomPerShot;
    [Tooltip("Maximum additional bloom in degrees")]
    public float maxBloom;
    [Tooltip("The distance the reticle moves to signify max bloom in pixels")]
    public float maxBloomReticleOffset;
    [Tooltip("Speed at which bloom returns to minimum in degrees per second")]
    public float bloomDecayPerSec;


    protected float bloom;

    [Header("Movement Multipliers")]
    // Default these values to 1 for normal bloom at those motion values
    public float standingMulti = 1.0f;
    public float walkingMulti = 1.0f;
    public float crouchingMulti = 1.0f;
    public float slidingMulti = 1.0f;
    public float airbornMulti = 1.0f;

    protected float movement01;
    protected float selectedMulti;

    [Header("Healing")]
    public float fullHeal;
    public float emptyHeal;

    [Header("Settings")]
    public bool usingCircularDistribution = true;
    public int maxAbilityCharges = 1;

    //[HideInInspector]
    public int displayedMaxCharges;
    public virtual Reticle InitiateReticle(Transform canvasParent, float reticleRotation = 0f)
    {
        if (reticle == null)
        {
            GameObject newRet = Instantiate(reticlePrefab, canvasParent);
            reticle = newRet.GetComponent<Reticle>();
        }

        reticle.SetZRotation(reticleRotation);
        return reticle;
    }
    public virtual void EquipWeapon(PlayerMovement p)
    {
        playerMovement = p;
        rb = p.gameObject.GetComponent<Rigidbody>();
        Ability = GetComponent<Ability>();

        if (reticle.reticleDirections.Count != reticle.reticlePieces.Count)
            Debug.LogError("Improper reticle setup on: " + gameObject.name);
    }
    private void Update()
    {
        BloomTick(Time.deltaTime);
    }
    private void BloomTick(float deltaTime)
    {
        bloom = Mathf.MoveTowards(bloom, 0f, bloomDecayPerSec * deltaTime);

        // Account for some weapons not having bloom, but still needing the code to run
        float interpolation;
        if (maxBloom <= 0f) interpolation = 1f;
        else interpolation = bloom / maxBloom;


        if (reticle != null) 
        { 
            for (int i = 0; i < reticle.reticlePieces.Count; i++)
            {
                Vector3 retDirection = new Vector3(Mathf.Cos(reticle.reticleDirections[i] * Mathf.Deg2Rad), Mathf.Sin(reticle.reticleDirections[i] * Mathf.Deg2Rad), 0f);

                reticle.reticlePieces[i].localPosition = Vector3.Lerp(reticle.reticlePositions[i], reticle.reticlePositions[i] + retDirection * maxBloomReticleOffset, interpolation);
            }
        }
    }
    protected void BloomOnShotFired(float bloomMulti = 1.0f)
    {
        bloom = Mathf.Clamp(bloom + bloomPerShot * bloomMulti, 0f, maxBloom);
    }
    protected float GetCurrentSpreadDegrees()
    {
        // Determines player's speed compared to desired speed for how much bloom movement multi to apply. This helps to 
        movement01 = playerMovement.moveSpeed / playerMovement.desiredMoveSpeed;
        // If slowing down, movement01 will cap at 50%
        if (movement01 > 1f) movement01 = 0.5f;

        float moveMulti = Mathf.Lerp(standingMulti, selectedMulti, Mathf.Clamp01(movement01));

        float total = (baseSpread + bloom) * moveMulti;
        return Mathf.Max(0f, total);

    }
    protected Quaternion GetSpreadRotation(float spreadDegrees)
    {
        if (spreadDegrees <= 0.0001f)
            return Quaternion.identity;

        if (usingCircularDistribution)
        {
            // Circular (Or cone) distribution feels better than square distribution
            Vector2 r = Random.insideUnitCircle * spreadDegrees;
            return Quaternion.Euler(r.y, r.x, 0f);
        }
        else
        {
            // Square distribution feels wider on angles but could be preferable in some cases
            float yaw = Random.Range(-spreadDegrees, spreadDegrees);
            float pitch = Random.Range(-spreadDegrees, spreadDegrees);
            return Quaternion.Euler(pitch, yaw, 0f);

        }

    }
    protected bool PlayerCanShoot()
    {
        // Determines how each state affects shooting, applying a set movement modifier or returning false is not allowed to shoot
        switch (playerMovement.moveState)
        {
            case PlayerMovement.MovementState.dashing:
                return false;
            case PlayerMovement.MovementState.walking:
                selectedMulti = walkingMulti;
                return true;
            case PlayerMovement.MovementState.crouching:
                selectedMulti = crouchingMulti;
                return true;
            case PlayerMovement.MovementState.sliding:
                selectedMulti = slidingMulti;
                return true;
            case PlayerMovement.MovementState.airborne:
                selectedMulti = airbornMulti;
                return true;
            default:
                return true;
        }
    }
    public float GetHealValue()
    {
        return Mathf.Lerp(emptyHeal, fullHeal, currentAmmo / maxAmmo);
    }
    public abstract void PrimaryBegin();
    public abstract void PrimaryRelease();
    public abstract void SecondaryBegin();
    public abstract void SecondaryRelease();
}
