using System.Collections;
using UnityEngine;

public class FlyerWeapon : EnemyWeapon
{
    private Coroutine shootRoutine;

    private IEnumerator WindShoot()
    {


        while (isShooting)
        {
            if (canShoot)
            {
                // Get direction of shot, applying bloom
                Quaternion rotation = Quaternion.LookRotation(target - projectileSpawnPoint.position, Vector3.up);
                float spreadDeg = GetCurrentSpreadDegrees();
                rotation = GetSpreadRotation(spreadDeg) * rotation;

                // Instantiate projectile and get it moving
                GameObject shot = Instantiate(projectile, projectileSpawnPoint.position, rotation);

                Projectile shotPro = shot.GetComponent<Projectile>();
                shotPro.origin = transform.position;

                // Adjust weapon bloom
                BloomOnShotFired();

                yield return new WaitForSeconds(shootCD);
            }
        }

        shootRoutine = null;
    }

    public override void PrimaryFire()
    {
        if (canShoot)
            isShooting = true;

        shootRoutine ??= StartCoroutine(WindShoot());

    }
    public override void PrimaryEnd()
    {
        isShooting = false;
    }
}
