using UnityEngine;
using System.Collections;

public class ShadowWeapon : Weapon
{
    [Header("Shadow Stats")]
    [Tooltip("Slashes per second")]
    public float slashRate;

    private ShadowCloak shadowCloak;
    private bool isSlashing;
    private float slashCD;
    private Coroutine slashRoutine;


    public override void EquipWeapon(PlayerMovement p)
    {
        base.EquipWeapon(p);
        //shadowCloak = GetComponent<ShadowCloak>();
        //shadowCloak.Setup(p);
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
                cutPro.fwdMomentum = playerVelocity;

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

    }
    public override void SecondaryRelease()
    {
        // No effect triggered
    }
}
