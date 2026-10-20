using UnityEngine;

[RequireComponent (typeof(Health))]
public class Enemy : MonoBehaviour
{
    public enum DetectionState { spotted, searching, confused }

    [Header("Player Detection")]
    public PlayerMovement player;
    public Transform perspectivePoint;
    [Min(1f)]
    public float maxDetectionRange;
    [Min(0f)]
    public float minDetectionRange;
    [Range(0f, 180f)]
    public float detectionAngle;
    public LayerMask sightBlockingMask;

    private DetectionState dState = DetectionState.searching;

    [Header("Feedback")]
    public Light spotLight;
    public Light pointLight;
    public Material searchMat;
    public Color searchingCol;
    public Color spottedCol;
    public Color confusedCol;
    public bool isInSmoke;
    public float confusionDuration;

    private Health healthScript;
    private MeshRenderer headMR;
    private int headMRIndex = -1;
    private float confusionTimer;

    private void Start()
    {
        if (!TryGetComponent<Health>(out healthScript))
            Debug.Log("No Health script attached to " + gameObject.name);
        else
        {
            headMR = perspectivePoint.GetComponent<MeshRenderer>();
            headMRIndex = healthScript.GetMeshRendererIndex(headMR);
        }
    }
    private void OnValidate()
    {
        spotLight.range = maxDetectionRange;
        spotLight.spotAngle = detectionAngle;
        spotLight.innerSpotAngle = detectionAngle;
        pointLight.range = minDetectionRange;
    }
    private void Update()
    {
        Detection();
    }
    private void Detection()
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
        switch (dState)
        {
            case DetectionState.spotted:
                spotLight.color = spottedCol;
                pointLight.color = spottedCol;
                searchMat.color = spottedCol;
                healthScript.SetBaseMat(searchMat, headMRIndex);
                perspectivePoint.transform.LookAt(player.transform.position);
                break;
            case DetectionState.searching:
                spotLight.color = searchingCol;
                pointLight.color = searchingCol;
                searchMat.color = searchingCol;
                healthScript.SetBaseMat(searchMat, headMRIndex);
                break;
            case DetectionState.confused:
                spotLight.color = confusedCol;
                pointLight.color = confusedCol;
                searchMat.color = confusedCol;
                healthScript.SetBaseMat(searchMat, headMRIndex);
                break;
            default:
                break;
        }
    }
    private void ShowDetectionArea()
    {
        
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
        float angle = Vector3.Angle(perspectivePoint.transform.forward, toPlayer);
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
        Vector3 rightSideLine = Quaternion.Euler(0f, detectionAngle / 2f, 0f) * perspectivePoint.transform.forward;
        Vector3 leftSideLine = Quaternion.Euler(0f, -detectionAngle / 2f, 0f) * perspectivePoint.transform.forward;
        Gizmos.DrawLine(perspectivePoint.position + rightSideLine * minDetectionRange, perspectivePoint.position + rightSideLine * maxDetectionRange);
        Gizmos.DrawLine(perspectivePoint.position + leftSideLine * minDetectionRange, perspectivePoint.position + leftSideLine * maxDetectionRange);
    }

}
