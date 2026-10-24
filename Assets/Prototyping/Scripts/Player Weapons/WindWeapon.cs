using UnityEngine;
using System.Collections;

public class WindWeapon : Weapon
{ 
    [Header("Wind Stats")]
    [Tooltip("Shots per second")]
    public float fireRate;

    private WindDash windDash;
    private float fireCD;
    private Coroutine shootRoutine;

    public override void EquipWeapon(PlayerMovement p)
    {
        base.EquipWeapon(p);
        weaponAnimators[(int)CharacterAnimators.Arms].SetBool("IsShadow",false);

        windDash = GetComponent<WindDash>();
        windDash.Setup(p);
        fireCD = 1f / fireRate;

    }
    private IEnumerator Fire()
    {
        while (isFiring)
        {
            // If statement prevents shooting while not in shootable state without ending the routine
            // This allows player to continue holding button while sprinting and dashing to begin shooting when entering a shootable state
            if (PlayerCanShoot())
            {
                // Get direction of shot, applying bloom
                Quaternion rotation = Quaternion.LookRotation(target - projectileSpawnPoint.position, Vector3.up);
                float spreadDeg = GetCurrentSpreadDegrees();
                rotation = GetSpreadRotation(spreadDeg) * rotation;

                // Instantiate projectile and get it moving
                GameObject shot = Instantiate(projectile, projectileSpawnPoint.position, rotation);

                Projectile shotPro = shot.GetComponent<Projectile>();

                // Adjust weapon bloom
                BloomOnShotFired();

                // Adjust ammo and check if there is enough to keep shooting
                currentAmmo -= ammoPerShot;
                if (currentAmmo <= 0f)
                    isFiring = false;

                yield return new WaitForSeconds(fireCD);
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

        shootRoutine ??= StartCoroutine(Fire());
    }
    public override void PrimaryRelease()
    {
        isFiring = false;
    }
    public override void SecondaryBegin()
    {
        windDash.Dash();
    }
    public override void SecondaryRelease()
    {
        // No effect triggered
    }
}
