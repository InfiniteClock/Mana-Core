using UnityEngine;

public class AudioManagement : MonoBehaviour
{
    public static AudioManagement Instance;

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

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
    }

    //Player
    public void PlayerWalk()
    {
        playerWalk.Play();
    }
    public void PlayerSprint()
    {
        playerSprint.Play();
    }
    public void PlayerCrouchWalk()
    {
        playerCrouchWalk.Play();
    }
    public void PlayerSlide()
    {
        playerSlide.Play();
    }
    public void PlayerJump()
    {
        playerJump.Play();
    }
    public void PlayerDeath()
    {
        playerDeath.Play();
    }
    public void PlayerRespawn()
    {
        playerRespawn.Play();
    }

    //ManaCore
    public void ManaCoreSpawn()
    {
        manacoreSpawn.Play();
    }
    public void ManaCoreGrab()
    {
        manacoreGrab.Play();
    }
    public void ManaCoreCrush()
    {
        manacoreCrush.Play();
    }

    //Weapons
    public void WeaponHitmarker()
    {
        weaponHitmarker.Play();
    }
    public void WeaponShadowAttack()
    {
        weaponShadowAttack.Play();
    }
    public void WeaponShadowUtility()
    {
        weaponShadowUtility.Play();
    }
    public void WeaponWindAttack()
    {
        weaponWindAttack.Play();
    }
    public void WeaponWindUtility()
    {
        weaponWindUtility.Play();
    }
}
