using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnemyWeapon : MonoBehaviour
{
    public enum DistributionType { circular, square, flatHorizontal, flatVertical }

    protected Enemy enemy;

    [Header("Shooting/Attacking")]
    public Transform projectileSpawnPoint;
    public GameObject projectile;
    public WeaponType coreType;
    public float fireRate;
    public float burstDelay;

    [HideInInspector]
    public Vector3 target;

    protected float shootCD;
    protected bool isShooting;
    protected bool canShoot = true;

    [Header("Accuracy")]
    public float baseSpread;
    public float bloomPerShot;
    public float maxBloom;
    public float bloomDecayPerSec;

    protected float bloom;

    [Header("Settings")]
    public DistributionType distributionType;


    private void Start()
    {
        enemy = GetComponent<Enemy>();
        shootCD = 1f / fireRate;
    }

    private void Update()
    {
        BloomTick(Time.deltaTime);

        // Check if the enemy is still alive first
        if (enemy.hasTriggeredDeath) canShoot = false;
        else
        {
            // If player is spotted, shoot at them
            if (enemy.dState == Enemy.DetectionState.spotted) PrimaryFire();
            // Otherwise, don't shoot
            else PrimaryEnd();
        }
    }
    private void BloomTick(float deltaTime)
    {
        bloom = Mathf.MoveTowards(bloom, 0f, bloomDecayPerSec * deltaTime);
    }
    protected void BloomOnShotFired(float bloomMulti = 1.0f)
    {
        bloom = Mathf.Clamp(bloom + bloomPerShot * bloomMulti, 0f, maxBloom);
    }
    protected float GetCurrentSpreadDegrees()
    {
        float total = (baseSpread + bloom);
        return Mathf.Max(0f, total);

    }
    protected Quaternion GetSpreadRotation(float spreadDegrees)
    {
        if (spreadDegrees <= 0.0001f)
            return Quaternion.identity;

        switch (distributionType)
        {
            // Circular (Or cone) distribution feels better than square distribution
            case DistributionType.circular:
                Vector2 r = Random.insideUnitCircle * spreadDegrees;
                return Quaternion.Euler(r.y, r.x, 0f);

            // Square distribution feels wider on angles but could be preferable in some cases
            case DistributionType.square:
                float yaw = Random.Range(-spreadDegrees, spreadDegrees);
                float pitch = Random.Range(-spreadDegrees, spreadDegrees);
                return Quaternion.Euler(pitch, yaw, 0f);

            // A flat horizontal distribution that never varies in yaw
            case DistributionType.flatHorizontal:
                float flatPitch = Random.Range(-spreadDegrees, spreadDegrees);
                return Quaternion.Euler(flatPitch, 0f, 0f);

            // A flat vertical distribution that never varies in pitch
            case DistributionType.flatVertical:
                float flatYaw = Random.Range(-spreadDegrees, spreadDegrees);
                return Quaternion.Euler(0f, flatYaw, 0f);
            default:
                return Quaternion.identity;
        }
    }
    public virtual void PrimaryFire() { }
    public virtual void PrimaryEnd() { }
}
