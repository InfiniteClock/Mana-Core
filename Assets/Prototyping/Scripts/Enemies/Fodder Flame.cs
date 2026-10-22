using System.Collections;
using UnityEngine;

public class FodderFlame : EnemyWeapon
{
    public bool shootStraightAhead;
    private Coroutine shootRoutine;

    private IEnumerator FlameBurst()
    {
        float timer = 0f;
        int burst = 0; 
        while (isShooting)
        {
            if (shootStraightAhead)
            {
                target = projectileSpawnPoint.position + projectileSpawnPoint.forward;
            }
            if (canShoot)
            {
                if (burst < 3)
                {
                    // Get direction of shot, applying bloom
                    Quaternion rotation = Quaternion.LookRotation(target - projectileSpawnPoint.position, Vector3.up);
                    float spreadDeg = GetCurrentSpreadDegrees();
                    rotation = GetSpreadRotation(spreadDeg) * rotation;

                    // Instantiate projectile and get it moving
                    GameObject shot = Instantiate(projectile, projectileSpawnPoint.position, rotation);

                    Projectile shotPro = shot.GetComponent<Projectile>();
                    shotPro.origin = transform;

                    // Adjust weapon bloom
                    BloomOnShotFired();

                    burst++;
                    yield return new WaitForSeconds(burstDelay);
                    timer += burstDelay;
                }
                else if (timer < fireRate)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    timer = 0f;
                    burst = 0;
                }
            }
            else
                yield return null;
        }

        shootRoutine = null;
    }

    public override void PrimaryFire()
    {
        if (canShoot)
            isShooting = true;

        shootRoutine ??= StartCoroutine(FlameBurst());

    }
    public override void PrimaryEnd()
    {
        isShooting = false;
    }
}
