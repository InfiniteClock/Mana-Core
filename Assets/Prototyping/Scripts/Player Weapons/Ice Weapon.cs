using System.Collections;
using UnityEngine;

public class IceWeapon : Weapon
{
    [Header("Ice Weapon Properties")]
    public float fireRate;
    public float rechargeTime;
    public float maxChargeDamage;
    public float maxChargeScaleMulti;
    public float maxChargeAmmoPerShot;
    public float maxChargeBloomMulti;

    private Coroutine shootRoutine;
    private Coroutine rechargeRoutine;
    private float shootCD;
    private float chargeTimer;
    private Projectile mountedProjectile;

    public override void EquipWeapon(PlayerMovement p)
    {
        base.EquipWeapon(p);
        weaponAnimators[(int)CharacterAnimators.Arms].SetBool("IsShadow", false);

        shootCD = 1f / fireRate;

        MountNewProjectile();
    }
    private void LateUpdate()
    {
        if (mountedProjectile != null) 
            if (!mountedProjectile.hasFired)
            {
                mountedProjectile.transform.position = projectileSpawnPoint.position;
                mountedProjectile.transform.rotation = projectileSpawnPoint.rotation;
            }
    }
    private IEnumerator Shoot()
    {

        while (isFiring)
        {
            // If statement prevents shooting while not in shootable state without ending the routine
            // This allows player to continue holding button to begin shooting when entering a shootable state
            if (PlayerCanShoot())
            {
                // Stop charging the projectile
                if (rechargeRoutine != null) 
                { 
                    StopCoroutine(rechargeRoutine);
                    rechargeRoutine = null;
                }

                // Remove the parent of the projectile - we no longer need to carry it around
                mountedProjectile.transform.SetParent(null);

                // Get direction of shot, applying bloom
                Quaternion rotation = Quaternion.LookRotation(target - projectileSpawnPoint.position, Vector3.up);
                float spreadDeg = GetCurrentSpreadDegrees();
                rotation = GetSpreadRotation(spreadDeg) * rotation;

                // Aims the projectile and fires it off
                mountedProjectile.transform.rotation = rotation;
                mountedProjectile.ToggleFireOn(true);

                // Removes the projectile from the weapon's control
                mountedProjectile = null;

                // Adjust weapon bloom
                BloomOnShotFired(Mathf.Lerp(1.0f, maxChargeBloomMulti, (chargeTimer - shootCD) / (rechargeTime - shootCD)));

                // Adjust ammo based on charge of shot and check if there is enough to keep shooting
                currentAmmo -= Mathf.Lerp(ammoPerShot, maxChargeAmmoPerShot, (chargeTimer - shootCD) / (rechargeTime - shootCD));
                if (currentAmmo <= 0f)
                    isFiring = false;
                else
                    MountNewProjectile();

                yield return new WaitForSeconds(shootCD);
            }
        }

        shootRoutine = null;
    }
    private void MountNewProjectile()
    {
        // Instantiates the new projectile and makes it a child of the weapon
        GameObject shot = Instantiate(projectile, projectileSpawnPoint.transform.position, projectileSpawnPoint.transform.rotation);

        mountedProjectile = shot.GetComponent<Projectile>();
        // Stops the projectile from moving or acting on lifetime properties
        mountedProjectile.ToggleFireOn(false);

        // Begin charging the new projectile
        rechargeRoutine ??= StartCoroutine(Recharge());
    }
    private IEnumerator Recharge()
    {
        chargeTimer = 0f;
        Vector3 startingScale = mountedProjectile.transform.localScale;
        float startingDmg = mountedProjectile.damage;
        mountedProjectile.transform.localScale = Vector3.zero;

        while (chargeTimer < rechargeTime && mountedProjectile != null)
        {
            // Initiates a new projectile
            if (chargeTimer < shootCD)
            {
                // Grows the new projectile to starting size over the cooldown time between shots
                mountedProjectile.transform.localScale = Vector3.Lerp(Vector3.zero, startingScale, chargeTimer / shootCD);
            }
            // Charges the projectile while not firing
            else
            {
                // Calculates the interpolation time, minus the minimum cooldown time per shot at autofire
                float i = (chargeTimer - shootCD) / (rechargeTime - shootCD);

                // Adjusts scale based on charge time
                mountedProjectile.transform.localScale = Vector3.Lerp(startingScale, startingScale * maxChargeScaleMulti, i);

                // Adjusts damage based on charge time
                mountedProjectile.damage = Mathf.Lerp(startingDmg, maxChargeDamage, i);
            }
            chargeTimer += Time.deltaTime;
            yield return null;
        }
        mountedProjectile.transform.localScale = startingScale * maxChargeScaleMulti;
        mountedProjectile.damage = maxChargeDamage;

        rechargeRoutine = null;
    }
    private void OnDestroy()
    {
        if (mountedProjectile != null) Destroy(mountedProjectile.gameObject);
    }
    public override void PrimaryBegin()
    {
        if (currentAmmo > 0f && PlayerCanShoot())
            isFiring = true;

        shootRoutine ??= StartCoroutine(Shoot());
    }
    public override void PrimaryRelease()
    {
        isFiring = false;
    }
    public override void SecondaryBegin()
    {
        // Create Ice Platform
    }
    public override void SecondaryRelease()
    {

    }
}
