using UnityEngine;

public class AudioManagement : MonoBehaviour
{
    [Space(10), Header("Player")]
    public AudioSource playerWalk;
    public AudioSource playerSprint;
    public AudioSource playerCrouchWalk;
    public AudioSource playerSlide;
    public AudioSource playerJump;
    public AudioSource playerDeath;
    public AudioSource playerRespawn;

    [Space(10), Header("Mana Core")]
    public AudioSource manacoreSpawn;
    public AudioSource manacoreGrab;
    public AudioSource manacoreCrush;

    [Space(10), Header("Weapons")]
    public AudioSource weaponHitmarker;
    public AudioSource weaponShadowAttack;
    public AudioSource weaponShadowUtility;
    public AudioSource weaponWindAttack;
    public AudioSource weaponWindUtility;

    //Player
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

    //ManaCore
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

    //Weapons
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
