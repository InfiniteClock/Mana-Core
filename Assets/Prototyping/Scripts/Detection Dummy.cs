using UnityEngine;

public class DetectionDummy : MonoBehaviour
{
    public enum DetectionState { spotted, searching, confused}

    [Header("Player Detection")]
    
    public Player player;
    public Transform perspectivePoint;
    public float maxDetectionRange;
    public float minDetectionRange;
    public float detectionAngle;
    public LayerMask sightBlockingMask;

    private DetectionState dState = DetectionState.searching;
    private MeshRenderer mr;

    [Header("Feedback")]
    public Material searchMat;
    public Material spottedMat;
    public Material confusedMat;
    public bool isInSmoke;
    public float confusionDuration;

    private float confusionTimer;

    private void Start()
    {
        mr = perspectivePoint.GetComponent<MeshRenderer>();
    }
    private void Update()
    {
        if (confusionTimer > 0) confusionTimer -= Time.deltaTime;
        else isInSmoke = false;

        // If player has been spotted
        if (CanDetectPlayer()) 
                dState = DetectionState.spotted;
        // Default state is searching
        else
            dState = DetectionState.searching;
        // Being smoked overrides other two states with confusion
        if (isInSmoke)
            dState = DetectionState.confused;

        // Set the material to reflect the current state
        switch(dState)
        {
            case DetectionState.spotted:
                mr.material = spottedMat;
                transform.LookAt(player.transform.position);
                break;
            case DetectionState.searching:
                mr.material = searchMat;
                break;
            case DetectionState.confused:
                mr.material = confusedMat;
                break;
            default:
                mr.material = null;
                break;
        }
    }
    private bool CanDetectPlayer()
    {
        // If the player is cloaked, must still be searching
        if (player.isCloaked) return false;

        // Get distance to player
        Vector3 toPlayer = player.transform.position - perspectivePoint.position;
        float distToPlayer = toPlayer.magnitude;

        // If player is outside of max detection range, must be in searching still
        if (distToPlayer > maxDetectionRange) return false;

        // If the player is not within the min radius or the sightline angle of the max radius, must still be searching
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > detectionAngle / 2f && distToPlayer > minDetectionRange) return false;

        // If the sightline to the player is NOT blocked by terrain, spot them
        return !Physics.Raycast(perspectivePoint.position, toPlayer.normalized, distToPlayer, sightBlockingMask);
        
    }
    public void SmokeBomb()
    {
        if (!isInSmoke) isInSmoke = true;
        if (confusionTimer <= 0) confusionTimer = confusionDuration;
    }

    private void OnDrawGizmos()
    {
        // Draws max radius of detection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(perspectivePoint.position, maxDetectionRange);

        // Draws minimum area of detection
        Gizmos.color = Color.wheat;
        Gizmos.DrawWireSphere(perspectivePoint.position, minDetectionRange);

        // Draws angle between min and max radius of detection
        Vector3 rightSideLine = Quaternion.Euler(0f, detectionAngle / 2f, 0f) * transform.forward;
        Vector3 leftSideLine = Quaternion.Euler(0f, -detectionAngle / 2f, 0f) * transform.forward;
        Gizmos.DrawLine(perspectivePoint.position + rightSideLine * minDetectionRange, perspectivePoint.position + rightSideLine * maxDetectionRange);
        Gizmos.DrawLine(perspectivePoint.position + leftSideLine * minDetectionRange, perspectivePoint.position + leftSideLine * maxDetectionRange);
    }
}
