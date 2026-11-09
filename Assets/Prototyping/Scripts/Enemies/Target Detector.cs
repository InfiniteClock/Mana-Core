using UnityEngine;

public class TargetDetector : MonoBehaviour
{

    [Header("Detection")]
    public GameObject target;
    public Transform eyePoint;
    [Min(1f)]
    public float maxDetectRange;
    [Min(0f)]
    public float minDetectRange;
    [Range(0f, 180f)]
    public float detectAngle;
    public float timeToDetect;
    public float timeToLoseDetection;
    public LayerMask detectionBlockingLayers;

    private float spotTimer;
    private float loseTimer;
    private PlayerMovement targetPlayer;

    [Header("Feedback")]
    public Light spotLight;
    public Light pointLight;

    [Header("Status Effects")]
    public float confusionDuration;
    public bool isConfused;

    private float confusionTimer;

    private void Start()
    {
        SetTarget(target);
        spotLight.range = maxDetectRange;
        spotLight.spotAngle = detectAngle;
        spotLight.innerSpotAngle = detectAngle;
        pointLight.range = minDetectRange;
    }
    private void OnValidate()
    {
        spotLight.range = maxDetectRange;
        spotLight.spotAngle = detectAngle;
        spotLight.innerSpotAngle = detectAngle;
        pointLight.range = minDetectRange;
    }
    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
        newTarget.TryGetComponent(out targetPlayer);
    }

    public GameObject UpdateDetectTarget()
    {
        // Cannot detect player if confused or player is cloaked
        if (CheckConfusion()) return null;
        if (targetPlayer != null) 
            if (targetPlayer.isCloaked) return null;

        // Get distance to target
        Vector3 vectToTarget = target.transform.position - eyePoint.position;
        float distToTarget = vectToTarget.magnitude;

        // Cannot detect target if outside of detection range
        if (distToTarget > maxDetectRange) return null;

        // Cannot detect target if within the max range, but outside the min range and sightline angle
        float angle = Vector3.Angle(eyePoint.transform.forward, vectToTarget);
        if (angle > detectAngle / 2f && distToTarget > minDetectRange) return null;

        // Lastly, cannot detect target through terrain
        return Physics.Raycast(eyePoint.position, vectToTarget.normalized, distToTarget, detectionBlockingLayers) ? null : target;

    }
    /// <summary>
    /// Determines if the target has been seen long enough to spot, or not seen long enough to lose. This creates small delays in decision making to smooth out AI behaviour
    /// </summary>
    /// <param name="detectsTarget">True if able to detect the target. False if not</param>
    /// <returns>True if spotted the target or losing the target. False if lost or spotting the target.</returns>
    public bool UpdateDetectTargetTimers(bool detectsTarget)
    {
        // There is currently a line of sight to the target
        if (detectsTarget)
        {
            // Reduce the loseTimer since we can see them
            if (loseTimer > 0f) loseTimer -= Time.deltaTime;

            // Is spotting target
            if (spotTimer < timeToDetect)
            {
                spotTimer += Time.deltaTime;
                return false;
            }
            // Has spotted target
            else
            {
                // We have spotted them, so reset the loseTimer
                loseTimer = 0f;
                return true;
            }

        }
        // There is NOT currently a line of sight to the target
        else
        {
            // Reduce the spotTimer since we can't see them
            if (spotTimer > 0f) spotTimer -= Time.deltaTime;

            // Is losing target
            if (loseTimer < timeToLoseDetection)
            {
                loseTimer += Time.deltaTime;
                return true;
            }
            // Has lost target
            else
            {
                // We have lost them, so reset the spotTimer
                spotTimer = 0f;
                return false;
            }
        }
    }
    private bool CheckConfusion()
    {
        if (confusionTimer > 0f)
        {
            confusionTimer -= Time.deltaTime;
            return isConfused = true;
        }
        else
        {
            return isConfused = false;
        }
    }
}
