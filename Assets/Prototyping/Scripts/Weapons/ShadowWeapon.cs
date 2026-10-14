using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class ShadowWeapon : Weapon
{
    [Header("Shadow Stats")]
    [Tooltip("Slashes per second")]
    public float slashRate;
    public float cloakSlashScaleMulti = 1f;
    public float delayPerSlashStage;
    [Tooltip("The distance in meters for offset from the preceding projectile. Based on (right, up, fwd) direction of (1,1,1)")]
    public Vector3 stageOffset;
    public GameObject midProjectile;
    public GameObject trailProjectile;


    private ShadowCloak shadowCloak;
    private bool isSlashing;
    private float slashCD;
    private Coroutine slashRoutine;

    public override void EquipWeapon(PlayerMovement p)
    {
        base.EquipWeapon(p);
        shadowCloak = GetComponent<ShadowCloak>();
        shadowCloak.Setup(p);
        slashCD = 1f / slashRate;
    }

    private IEnumerator Slash()
    {
        while (isSlashing)
        {
            // If statement prevents slashing while not in slashable state, without ending the routine
            // This allows player to continue holding button while sprinting and dashing to begin slashing when entering a slashable state
            if (PlayerCanShoot())
            {
                // Get direction of slash. No bloom applied but still needs to call function for correct direction
                Quaternion rotation = Quaternion.LookRotation(target - projectileSpawnPoint.position, Vector3.up);
                rotation = GetSpreadRotation(1f) * rotation;

                // Instantiate projectile and get it moving
                GameObject cut = Instantiate(projectile, projectileSpawnPoint.position, rotation);

                Projectile cutPro = cut.GetComponent<Projectile>();

                // Initiate the follow up projectiles
                StartCoroutine(SlashDelay(cutPro));

                // Check if cloaked and adjust slash stats as necessary
                if (playerMovement.isCloaked)
                    cut.transform.localScale = cut.transform.localScale * cloakSlashScaleMulti;
                
                // Adjust ammo and check if there is enough to keep shooting
                currentAmmo -= ammoPerShot;
                if (currentAmmo <= 0f)
                    isSlashing = false;

                yield return new WaitForSeconds(slashCD);
            }
            else 
                yield return null;
        }
        slashRoutine = null;
    }
    private IEnumerator SlashDelay(Projectile cutPro)
    {
        Vector3 downOffset = new Vector3(stageOffset.x, -stageOffset.y, stageOffset.z);
        
        // Delay, then spawn in the mid projectiles
        yield return new WaitForSeconds(delayPerSlashStage);

        GameObject midSlash1 = Instantiate(midProjectile, cutPro.transform.position, cutPro.transform.rotation, cutPro.transform);
        midSlash1.transform.localPosition = stageOffset;

        GameObject midSlash2 = Instantiate(midProjectile, cutPro.transform.position, cutPro.transform.rotation, cutPro.transform);
        midSlash2.transform.localPosition = downOffset;

        // Delay, then spawn in the trail projectiles
        yield return new WaitForSeconds(delayPerSlashStage);

        GameObject trailSlash1 = Instantiate(trailProjectile, cutPro.transform.position, cutPro.transform.rotation, cutPro.transform);
        trailSlash1.transform.localPosition = stageOffset * 1.5f;

        GameObject trailSlash2 = Instantiate(trailProjectile, cutPro.transform.position, cutPro.transform.rotation, cutPro.transform);
        trailSlash2.transform.localPosition = downOffset * 1.5f;
    }

    public override void PrimaryBegin()
    {
        if (currentAmmo > 0f && PlayerCanShoot())
            isSlashing = true;

        slashRoutine ??= StartCoroutine(Slash());
    }
    public override void PrimaryRelease()
    {
        isSlashing = false;
    }
    public override void SecondaryBegin()
    {
        shadowCloak.Cloak();
    }
    public override void SecondaryRelease()
    {
        // No effect triggered
    }
}
