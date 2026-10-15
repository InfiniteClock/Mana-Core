using UnityEngine;

public class AudioManagement : MonoBehaviour
{
    public AudioSource playerWalk;
    public AudioSource playerSprint;
    public AudioSource playerCrouchWalk;
    public AudioSource playerSlide;
    public AudioSource playerJump;

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
}
