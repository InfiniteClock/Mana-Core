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

    public override void EquipWeapon(PlayerMovement p)
    {
        base.EquipWeapon(p);
        weaponAnimators[(int)CharacterAnimators.Arms].SetBool("IsShadow", false);

        //windDash = GetComponent<WindDash>();
        //windDash.Setup(p);
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
        
    }
    public override void SecondaryRelease()
    {
        // No effect triggered
    }
}
