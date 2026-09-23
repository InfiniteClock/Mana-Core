using System.Collections;
using UnityEditor.Experimental.GraphView;
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
    private bool charging;
    private Coroutine gunFireRoutine;

    private void Update()
    {
        if (gType == GunType.Fire)
        {
            if (charging)
                gunFireRoutine ??= StartCoroutine(FireBurst());
        }
        if (gType == GunType.Ice)
        {
            if (charging)
                iceCharge = Mathf.Min(iceCharge + Time.deltaTime, maxIceCharge);
            else
                iceCharge = Mathf.Max(iceCharge - Time.deltaTime, 0f);
        }
    }
    public void Charge()
    {
        //Debug.Log("Charging...");
        
        charging = true;
        
    }
    public void Shoot()
    {
        //Debug.Log("Shooting...");
        charging = false;

        if (gType == GunType.Fire)
        {
            // No release effect
        }
        else if (gType == GunType.Ice)
        {
            gunFireRoutine ??= StartCoroutine(IceSpear());
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
        proj.transform.localScale *= (iceCharge / maxIceCharge * 2f) + 1f;

        Projectile p = proj.GetComponent<Projectile>();
        p.travelSpeed *= (iceCharge / maxIceCharge * 2f) + 1f;
        p.damage *= (iceCharge / maxIceCharge * 2f) + 1f;

        iceCharge = 0f;

        yield return new WaitForSeconds(cooldown);
        gunFireRoutine = null;
    }
}
