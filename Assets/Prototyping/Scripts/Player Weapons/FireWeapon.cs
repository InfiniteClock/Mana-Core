using UnityEngine;
using System.Collections;

public class FireWeapon : Weapon
{
    [Header("Fire Stats")]
    public float fireRate;
    public int burstNumber;
    public float burstDelay;


    private float fireCD;
    private Coroutine shootRoutine;
    private FireBlast fireBlast;

    [Header("Animator")]
    private string side;
    private Animator weaponSide;
    private Animator coreSide;
    private Animator arms;

    public override void EquipWeapon(PlayerMovement p)
    {
        base.EquipWeapon(p);

        side = isRightWeapon ? "R" : "L";

        arms = weaponAnimators[(int)CharacterAnimators.Arms];
        
        if (isRightWeapon)
        {
            weaponSide = weaponAnimators[(int)CharacterAnimators.WisperR];
            coreSide = weaponAnimators[(int)CharacterAnimators.CoreR];
        } else
        {
            weaponSide = weaponAnimators[(int)CharacterAnimators.WisperL];
            coreSide = weaponAnimators[(int)CharacterAnimators.CoreL];
        }

        weaponSide.SetTrigger("Equip");
        weaponSide.SetBool("IsFire", true);
        coreSide.SetTrigger("IsFire");
        arms.SetBool("IsFire",true);
        arms.SetBool($"WeaponDestroy.{side}", false);
        arms.SetTrigger($"FW_Equip.{side}");

        fireBlast = GetComponent<FireBlast>();
        fireBlast.Setup(p);
        fireCD = 1f / fireRate;

    }

    private IEnumerator BurstFire()
    {
        int burst = 0;
        float burstCDOffset = burstDelay * burstNumber;
        
        while (isFiring)
        {
            // If statement prevents shooting while not in shootable state without ending the routine
            // This allows player to continue holding button to begin shooting when entering a shootable state
            if (PlayerCanShoot())
            {
                while (burst < burstNumber)
                {
                    if (burst == 0)
                    {
                        weaponSide.SetTrigger("Shoot");
                        arms.SetTrigger($"FW_Shoot.{side}");
                    }

                    // Get direction of shot, applying bloom
                    Quaternion rotation = Quaternion.LookRotation(target - projectileSpawnPoint.position, Vector3.up);
                    float spreadDeg = GetCurrentSpreadDegrees();
                    rotation = GetSpreadRotation(spreadDeg) * rotation;

                    // Instantiate projectile and get it moving
                    GameObject shot = Instantiate(projectile, projectileSpawnPoint.position, rotation);

                    // Adjust weapon bloom
                    BloomOnShotFired();


                    // Adjust ammo and check if there is enough to keep shooting
                    currentAmmo -= ammoPerShot;
                    if (currentAmmo <= 0f)
                        isFiring = false;

                    burst++;
                    // Waits for the short duration of the burst delay before firing the next projectile
                    yield return new WaitForSeconds(burstDelay);
                }
                burst = 0;
                // Waits for the remainder of the cooldown, minus existing cooldown that has applied throughout the burst
                yield return new WaitForSeconds(fireCD - burstCDOffset);   
            }
            else
                yield return null;
        }
        shootRoutine = null;
    }

    public override void PrimaryBegin()
    {
        if (currentAmmo > 0f && PlayerCanShoot())
            isFiring = true;

        shootRoutine ??= StartCoroutine(BurstFire());
    }
    public override void PrimaryRelease()
    {
        isFiring = false;
    }
    public override void SecondaryBegin()
    {
        fireBlast.Blast();
    }
    public override void SecondaryRelease()
    {
        // No effect triggered
    }
}
