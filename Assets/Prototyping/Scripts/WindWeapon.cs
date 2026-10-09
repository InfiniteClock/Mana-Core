using UnityEngine;
using System.Collections;

public class WindWeapon : Weapon
{ 
    [Header("Gun Stats")]
    [Tooltip("Shots per second")]
    public float fireRate;

    private bool isFiring;
    private float fireCD;
    private Coroutine shootRoutine;

    private void OnValidate()
    {
        fireCD = 1f / fireRate;
    }
    private IEnumerator Fire()
    {
        while (isFiring)
        {
            // Get direction of shot, applying bloom
            Vector3 dir = (target - transform.position).normalized;
            float spreadDeg = GetCurrentSpreadDegrees(movingMulti, movement01);
            Quaternion rotation = Quaternion.Euler(dir.x, dir.y, dir.z);
            rotation = GetSpreadRotation(spreadDeg) * rotation;

            // Instantiate projectile and get it moving
            GameObject shot = Instantiate(projectile, projectileSpawnPoint.position, rotation);
            shot.transform.forward = dir;
            Projectile shotPro = shot.GetComponent<Projectile>();
            shotPro.fwdMomentum = playerVelocity.z;

            // Adjust weapon bloom
            BloomOnShotFired();

            // Adjust ammo and check if there is enough to keep shooting
            currentAmmo -= ammoPerShot;
            UpdateAmmo();
            if (currentAmmo <= 0f) isFiring = false;

            yield return new WaitForSeconds(fireCD);
        }
        shootRoutine = null;
    }

    public override void PrimaryBegin()
    {
        if (currentAmmo > 0f) isFiring = true;
        shootRoutine ??= StartCoroutine(Fire());
    }
    public override void PrimaryRelease()
    {
        isFiring = false;
    }
    public override void SecondaryBegin()
    {
        throw new System.NotImplementedException();
    }
    public override void SecondaryRelease()
    {
        throw new System.NotImplementedException();
    }
}
