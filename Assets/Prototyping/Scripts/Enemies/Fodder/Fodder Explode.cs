using System.Collections;
using UnityEngine;

public class FodderExplode : EnemyWeapon
{
    public float timeToExplode;
    private Coroutine explodeRoutine;

    private IEnumerator Explode()
    {
        while (isShooting)
        {
            // Perform any triggering animations or effects here

            yield return new WaitForSeconds(timeToExplode);
            // Actually blow up
            Detonate();
        }

        explodeRoutine = null;
    }
    public void Detonate()
    {
        // Instantiate projectile and get it moving
        GameObject shot = Instantiate(projectile, projectileSpawnPoint.position, Quaternion.identity);
        Projectile shotPro = shot.GetComponent<Projectile>();
        shotPro.origin = transform.position;

        // Damages the enemy for all of its remaining health
        enemy.health.TakeDamage(enemy.health.currentHealth);
    }
    public override void PrimaryFire()
    {
        if (canShoot)
            isShooting = true;

        explodeRoutine ??= StartCoroutine(Explode());
    }
    public override void PrimaryEnd()
    {
        isShooting= false;
    }
}
