using System.Collections;
using UnityEngine;

public class FodderMelee : EnemyWeapon
{
    private Coroutine meleeRoutine;

    private IEnumerator MeleeAttack()
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

                yield return new WaitForSeconds(shootCD);
            }
        }

        meleeRoutine = null;
    }

    public override void PrimaryFire()
    {
        if (canShoot)
            isShooting = true;

        meleeRoutine ??= StartCoroutine(MeleeAttack());
    }
    public override void PrimaryEnd()
    {
        isShooting = false;
    }
}
