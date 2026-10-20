using UnityEngine;

public class AudioManagement : MonoBehaviour
{
    public AudioSource playerWalk;
    public AudioSource playerSprint;
    public AudioSource playerCrouchWalk;
    public AudioSource playerSlide;
    public AudioSource playerJump;
    public AudioSource playerDeath;
    public AudioSource playerRespawn;

    public AudioSource manacoreSpawn;
    public AudioSource manacoreGrab;
    public AudioSource manacoreCrush;

    public AudioSource weaponHitmarker;
    public AudioSource weaponShadowAttack;
    public AudioSource weaponShadowUtility;
    public AudioSource weaponWindAttack;
    public AudioSource weaponWindUtility;


    void PlayerWalk()
    {
        playerWalk.Play();
    }
    void PlayerSprint()
    {
        playerSprint.Play();
    }
    void PlayerCrouchWalk()
    {
        playerCrouchWalk.Play();
    }
    void PlayerSlide()
    {
        playerSlide.Play();
    }
    void PlayerJump()
    {
        playerJump.Play();
    }
    void PlayerDeath()
    {
        playerDeath.Play();
    }
    void PlayerRespawn()
    {
        playerRespawn.Play();
    }

    void ManaCoreSpawn()
    {
        manacoreSpawn.Play();
    }
    void ManaCoreGrab()
    {
        manacoreGrab.Play();
    }
    void ManaCoreCrush()
    {
        manacoreCrush.Play();
    }

    void WeaponHitmarker()
    {
        weaponHitmarker.Play();
    }
    void WeaponShadowAttack()
    {
        weaponShadowAttack.Play();
    }
    void WeaponShadowUtility()
    {
        weaponShadowUtility.Play();
    }
    void WeaponWindAttack()
    {
        weaponWindAttack.Play();
    }
    void WeaponWindUtility()
    {
        weaponWindUtility.Play();
    }
}
