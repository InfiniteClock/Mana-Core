using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Header("References")]
    public Transform orientation {  get; protected set; }
    public Transform playerCam { get; protected set; }

    protected Rigidbody rb;
    protected PlayerMovement playerMovement;
    protected PlayerWeapons playerWeapons;
    protected Weapon weapon;

    [Header("Cooldown")]
    public float cooldown;
    //[HideInInspector]
    public float cooldownTimer;


    public void Setup(PlayerMovement p)
    {
        weapon = GetComponent<Weapon>();
        playerMovement = p;
        playerWeapons = p.GetComponent<PlayerWeapons>();
        rb = playerMovement.GetComponent<Rigidbody>();
        orientation = p.orientation;
        playerCam = p.playerCam.transform;
    }
}
