using System.Collections;
using UnityEngine;

public class WindDash : Ability
{
    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;
    public float maxDashYSpeed;
    private Vector3 delayedForceToApply;

    [Header("Settings")]
    public bool useCameraForward;
    public bool omnidirectionalDash;
    public bool resetVelocity;
    public void Dash()
    {
        // Returns if dash has no remaining charges or is on internal buffer from other weapon
        if (playerWeapons.GetAbilityCharges(weapon.isRightWeapon) < 1 || playerWeapons.bufferTimer > 0) return;
        else
        {
            // Reset the cooldown, 
            cooldownTimer = cooldown;
            playerWeapons.UseAbilityCharge(weapon.isRightWeapon);

            if (playerWeapons.hasTwinWeapons)
                playerWeapons.bufferTimer = playerWeapons.dashBuffer;
        }

        playerMovement.isDashing = true;
        playerMovement.maxYSpeed = maxDashYSpeed;

        Transform forwardT;

        if (useCameraForward)
            forwardT = playerCam;
        else 
            forwardT = orientation;

        Vector3 forceToApply = GetDirection(forwardT) * dashForce + orientation.up * dashUpwardForce;

        // Triggers the dash after a very short delay to account for player script order of operations
        delayedForceToApply = forceToApply;

        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        if (resetVelocity)
        {
            // Cancels all previous movement first
            rb.linearVelocity = Vector3.zero;
        }

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        playerMovement.isDashing = false;
        playerMovement.maxYSpeed = 0;
    }
    private Vector3 GetDirection(Transform forwardT)
    {
        Vector3 input = new Vector3(playerMovement.rawMoveInput.x, 0f, playerMovement.rawMoveInput.y);

        Vector3 direction = new Vector3();

        // Dashes forward, or in any input direction, depending on setting
        if (omnidirectionalDash)
            direction = input.z * forwardT.forward + forwardT.right * input.x;
        else
            direction = forwardT.forward;

        if (input.magnitude == 0f)
            direction = forwardT.forward;

        return direction.normalized;
    }
}
