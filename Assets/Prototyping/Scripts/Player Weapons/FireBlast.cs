using UnityEngine;

public class FireBlast : Ability
{
    [Header("Blast Effect")]
    public float blastForce;
    public float blastUpwardForce;
    public float maxBlastYSpeed;
    public float blastDuration;
    public Transform spawnpoint;
    public GameObject blastPrefab;

    private Vector3 delayedForceToApply;

    [Header("Camera")]
    public float cameraFovValue;
    private float initialCameraValue;

    [Header("Settings")]
    public bool resetVelocity;

    public void Blast()
    {
        // Returns if dash has no remaining charges or is on internal buffer from other weapon
        if (playerWeapons.GetAbilityCharges(weapon.isRightWeapon) < 1 || playerWeapons.bufferTimer > 0) return;

        InitiateAbility();

        playerMovement.isBlasting = true;
        playerMovement.maxYSpeed = maxBlastYSpeed;
        delayedForceToApply = playerCam.forward;
        initialCameraValue = playerWeapons.playerCam.fieldOfView;

        SpawnBlast();

        Invoke(nameof(DelayedBlastForce), 0.1f);

        Invoke(nameof(ResetBlast), blastDuration);
    }
    private void DelayedBlastForce()
    {
        if (resetVelocity)
        {
            // Cancels all previous movement first
            rb.linearVelocity = Vector3.zero;
        }
        playerWeapons.playerCam.fieldOfView = cameraFovValue;

        rb.AddForce(-delayedForceToApply * blastForce, ForceMode.Impulse);
        rb.AddForce(Vector3.up * blastUpwardForce, ForceMode.Impulse);
    }
    private void ResetBlast()
    {
        playerMovement.isBlasting = false;
        playerMovement.maxYSpeed = 0;
        playerWeapons.playerCam.fieldOfView = initialCameraValue;
    }
    private void SpawnBlast()
    {
        GameObject boom = Instantiate(blastPrefab, spawnpoint.position, playerCam.rotation);
    }
}
