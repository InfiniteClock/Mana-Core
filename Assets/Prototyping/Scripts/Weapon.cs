using System.Collections;
using UnityEngine;
public enum GunType { Fire, Ice }
public class Weapon : MonoBehaviour
{
    public GameObject projectile;
    public GunType gType;
    public float cooldown;
    public float burstDelay;

    private Coroutine gunFireRoutine;
    public void Charge()
    {
        if (gType == GunType.Fire)
        {
            // Nothing happens
        }
        else if (gType == GunType.Ice)
        {
            Debug.Log("Charging Ice Projectile...");
        }
    }
    public void Shoot()
    {
        if (gType == GunType.Fire)
        {
            // Shoot Gun
            gunFireRoutine ??= StartCoroutine(FireBurst());
        }
        else if (gType == GunType.Ice)
        {

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
            yield return new WaitForSeconds(burstDelay);
            timer += burstDelay;
        }
        while (timer < cooldown)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        gunFireRoutine = null;
    }
}
