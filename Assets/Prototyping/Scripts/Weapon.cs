using System.Collections;
using UnityEngine;
public enum GunType { Fire, Ice }
public class Weapon : MonoBehaviour
{
    public GameObject projectile;
    public GunType gType;
    public float cooldown;
    public float burstFireDelay;
    public float maxIceCharge;

    public float iceCharge;
    private bool iceCharging;
    private Coroutine gunFireRoutine;

    private void Update()
    {
        if (iceCharging)
            iceCharge = Mathf.Min(iceCharge + Time.deltaTime, maxIceCharge);
        else
            iceCharge = Mathf.Max(iceCharge - Time.deltaTime, 0f);
        
    }
    public void Charge()
    {
        if (gType == GunType.Fire)
        {
            // Shoot Fire Gun
            gunFireRoutine ??= StartCoroutine(FireBurst());
        }
        else if (gType == GunType.Ice)
        {
            if (gunFireRoutine != null)
                iceCharging = true;
        }
    }
    public void Shoot()
    {
        if (gType == GunType.Fire)
        {
            // No release effect
        }
        else if (gType == GunType.Ice)
        {
            gunFireRoutine ??= StartCoroutine(IceSpear());
            iceCharging = false;
        }
    }
    IEnumerator FireBurst()
    {
        float timer = 0f;
        int burst = 0;

        while (burst < 3)
        {
            GameObject proj = Instantiate(projectile);
            proj.transform.position = transform.position;
            proj.transform.forward = transform.forward;
            burst++;
            yield return new WaitForSeconds(burstFireDelay);
            timer += burstFireDelay;
        }
        while (timer < cooldown)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        gunFireRoutine = null;
    }
    IEnumerator IceSpear()
    {
        GameObject proj = Instantiate(projectile);
        proj.transform.position = transform.position;
        proj.transform.forward = transform.forward;
        proj.transform.localScale *= (iceCharge / maxIceCharge) + 1f;

        Projectile p = proj.GetComponent<Projectile>();
        p.travelSpeed *= (iceCharge / maxIceCharge) + 1f;
        p.damage *= (iceCharge / maxIceCharge) + 1f;

        yield return new WaitForSeconds(cooldown);
        gunFireRoutine = null;
    }
}
