using UnityEngine;

public class DetectionDummy : MonoBehaviour
{
    public enum DetectionState { spotted, searching, confused}

    [Header("Player Detection")]
    public Transform player;
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

    private void Start()
    {
        mr = perspectivePoint.GetComponent<MeshRenderer>();
    }
    private void Update()
    {
        if (CanDetectPlayer())
            dState = DetectionState.spotted;
        else
            dState = DetectionState.searching;

        switch(dState)
        {
            case DetectionState.spotted:
                mr.material = spottedMat;
                transform.LookAt(player);
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
        Vector3 toPlayer = player.position - perspectivePoint.position;
        float distToPlayer = toPlayer.magnitude;

        // If player is outside of max detection range, must be in searching still
        if (distToPlayer > maxDetectionRange) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > detectionAngle / 2f && distToPlayer > minDetectionRange) return false;

        return !Physics.Raycast(perspectivePoint.position, toPlayer.normalized, distToPlayer, sightBlockingMask);
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.wheat;
        Gizmos.DrawWireSphere(perspectivePoint.position, minDetectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(perspectivePoint.position, maxDetectionRange);

        Vector3 rightSideLine = Quaternion.Euler(0f, detectionAngle / 2f, 0f) * transform.forward;
        Vector3 leftSideLine = Quaternion.Euler(0f, -detectionAngle / 2f, 0f) * transform.forward;
        Gizmos.DrawLine(perspectivePoint.position + rightSideLine * minDetectionRange, perspectivePoint.position + rightSideLine * maxDetectionRange);
        Gizmos.DrawLine(perspectivePoint.position + leftSideLine * minDetectionRange, perspectivePoint.position + leftSideLine * maxDetectionRange);
    }
}
